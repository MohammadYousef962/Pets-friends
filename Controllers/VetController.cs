using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Data.ViewModels;
using Pets_friends.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Pets_friends.Controllers
{
    public class VetController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VetController(AppDbContext context, UserManager<UserAccount> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Vet")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.VetProfiles
                .Include(p => p.UserAccount)
                .Include(p => p.Reviews).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return RedirectToAction(nameof(Create));

            ViewData["VetImageUrl"] = profile.ImageUrl;
            ViewData["VetName"] = user.FullName ?? "Veterinarian";

            // 1. PENDING APPOINTMENTS (For the Left Queue)
            var pendingAppointments = await _context.Appointments
                .Include(a => a.ClientProfile).ThenInclude(c => c.UserAccount)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Where(a => a.VetProfileId == profile.Id && a.Status == "Pending")
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            // 2. THE FIX: CONFIRMED UPCOMING (For the Right Side Schedule Panel)
            var now = DateTime.Now;
            var upcomingAppointments = await _context.Appointments
                .Include(a => a.ClientProfile).ThenInclude(c => c.UserAccount)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Where(a => a.VetProfileId == profile.Id && a.Status == "Confirmed" && a.AppointmentDate >= now)
                .OrderBy(a => a.AppointmentDate)
                .Take(15) // Pull top 15 closest appointments
                .ToListAsync();

            // Pass the upcoming appointments directly to the View via ViewBag
            ViewBag.UpcomingAppointments = upcomingAppointments;

            var vm = new VetDashboardVM
            {
                Profile = profile,
                PendingAppointments = pendingAppointments,
                RecentReviews = profile.Reviews?
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToList() ?? new List<VetReview>()
            };

            return View(vm);
        }
        // --------------------------------------------------------
        // PUBLIC PROFILE VIEW
        // --------------------------------------------------------
        [AllowAnonymous]
        public async Task<IActionResult> Profile(int? id)
        {
            VetProfile? vet = null;
            int servedPets = 0;

            if (id.HasValue && id.Value > 0)
            {
                vet = await _context.VetProfiles
                    .Include(v => v.UserAccount)
                    .Include(v => v.Schedule)
                    .Include(v => v.Reviews).ThenInclude(r => r.Reviewer)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vet != null)
                {
                    servedPets = await _context.Appointments
                        .CountAsync(a => a.VetProfileId == vet.Id && a.Status == "Completed");
                }
            }
            else if (User.Identity?.IsAuthenticated == true && User.IsInRole("Vet"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    vet = await _context.VetProfiles
                        .Include(v => v.UserAccount)
                        .Include(v => v.Schedule)
                        .Include(v => v.Reviews).ThenInclude(r => r.Reviewer)
                        .FirstOrDefaultAsync(v => v.UserAccountId == user.Id);

                    if (vet != null)
                    {
                        servedPets = await _context.Appointments
                            .CountAsync(a => a.VetProfileId == vet.Id && a.Status == "Completed");
                    }
                }

                if (vet == null) return RedirectToAction(nameof(Create));
            }

            if (vet == null) return NotFound();

            // Pass layout avatar data
            ViewData["VetImageUrl"] = vet.ImageUrl;
            ViewData["VetName"] = vet.UserAccount?.FullName ?? "Veterinarian";
            ViewData["ServedPetsCount"] = servedPets;

            // Fetch client's pets if a client is viewing the profile (for the booking modal)
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == currentUser.Id);
                    if (clientProfile != null)
                    {
                        ViewBag.MyPets = await _context.Pets
                            .Where(p => p.ClientProfileId == clientProfile.Id)
                            .ToListAsync();
                    }
                }
            }

            return View(vet);
        }

        // CLIENT: BOOK APPOINTMENT

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(int vetId, List<int> petIds, DateTime appointmentDate, string serviceName, string notes, bool isUrgent = false)
        {
            var user = await _userManager.GetUserAsync(User);
            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);

            if (clientProfile == null)
            {
                TempData["ErrorMessage"] = "You must complete your Client Profile before booking appointments.";
                return RedirectToAction("Profile", new { id = vetId });
            }

            if (petIds == null || !petIds.Any())
            {
                TempData["ErrorMessage"] = "You must select at least one pet for the appointment.";
                return RedirectToAction("Profile", new { id = vetId });
            }

            // --- THE FIX: Separate REAL pets from the New Pet (0) Placeholder ---
            var realPetIds = petIds.Where(id => id > 0).ToList();

            // Only create a New Patient Request if they passed 0 AND selected no real pets
            if (!realPetIds.Any() && petIds.Contains(0))
            {
                var appointment = new Appointment
                {
                    VetProfileId = vetId,
                    ClientProfileId = clientProfile.Id,
                    PetId = null, // Leaves it null for the Vet to verify
                    AppointmentDate = appointmentDate,
                    Status = "Pending",
                    Notes = $"Service: {serviceName} | Notes: {notes}",
                    IsUrgent = isUrgent
                };

                _context.Appointments.Add(appointment);
            }
            else
            {
                // Loop through the REAL selected pets only
                foreach (var pId in realPetIds)
                {
                    var appointment = new Appointment
                    {
                        VetProfileId = vetId,
                        ClientProfileId = clientProfile.Id,
                        PetId = pId, // Assigns the REAL pet!
                        AppointmentDate = appointmentDate,
                        Status = "Pending",
                        Notes = $"Service: {serviceName} | Notes: {notes}",
                        IsUrgent = isUrgent
                    };

                    _context.Appointments.Add(appointment);
                }
            }

            await _context.SaveChangesAsync();

            if (isUrgent)
            {
                TempData["SuccessMessage"] = "URGENT Appointment request sent! The clinic will be notified immediately.";
            }
            else
            {
                TempData["SuccessMessage"] = "Appointment requested successfully!";
            }

            return RedirectToAction("Profile", new { id = vetId });
        }

        // --------------------------------------------------------
        // DASHBOARD: ACCEPT / DECLINE APPOINTMENTS
        // --------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Vet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentGroupStatus(string appointmentIds, string newStatus)
        {
            var user = await _userManager.GetUserAsync(User);
            var vetProfile = await _context.VetProfiles.FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (vetProfile == null || string.IsNullOrEmpty(appointmentIds)) return RedirectToAction("Dashboard");

            var idsList = appointmentIds.Split(',').Select(int.Parse).ToList();
            var appointments = await _context.Appointments.Where(a => idsList.Contains(a.Id)).ToListAsync();

            if (!appointments.Any()) return RedirectToAction("Dashboard");

            // --- STRICT 30-MINUTE GAP VALIDATION ---
            if (newStatus == "Confirmed")
            {
                var requestedTime = appointments.First().AppointmentDate;
                var minTime = requestedTime.AddMinutes(-29);
                var maxTime = requestedTime.AddMinutes(29);

                bool hasConflict = await _context.Appointments.AnyAsync(a =>
                    a.VetProfileId == vetProfile.Id &&
                    a.Status == "Confirmed" &&
                    a.AppointmentDate >= minTime &&
                    a.AppointmentDate <= maxTime);

                if (hasConflict)
                {
                    TempData["ErrorMessage"] = "Time Slot Conflict! You already have a confirmed appointment within 30 minutes of this request.";
                    return RedirectToAction("Dashboard");
                }
            }

            foreach (var apt in appointments)
            {
                apt.Status = newStatus;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = newStatus == "Confirmed" ? "Appointment Successfully Scheduled!" : "Appointment Request Declined.";

            return RedirectToAction("Dashboard");
        }

        // --------------------------------------------------------
        // CLIENT: SUBMIT OR UPDATE REVIEW
        // --------------------------------------------------------
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int vetId, int rating, string comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 1. Check if the user already has a review for this specific Vet
            var existingReview = await _context.VetReviews
                .FirstOrDefaultAsync(r => r.VetProfileId == vetId && r.ReviewerId == user.Id);

            if (existingReview != null)
            {
                // 2. UPDATE mode: Just change the text, stars, and refresh the date
                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.CreatedAt = DateTime.Now;

                TempData["SuccessMessage"] = "Your review has been successfully updated!";
            }
            else
            {
                // 3. CREATE mode: Add a brand new review
                var review = new VetReview
                {
                    VetProfileId = vetId,
                    ReviewerId = user.Id,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };

                _context.VetReviews.Add(review);
                TempData["SuccessMessage"] = "Thank you! Your review has been published.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Profile", new { id = vetId });
        }

        // --------------------------------------------------------
        // CLIENT: DELETE REVIEW
        // --------------------------------------------------------
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int vetId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingReview = await _context.VetReviews
                .FirstOrDefaultAsync(r => r.VetProfileId == vetId && r.ReviewerId == user.Id);

            if (existingReview != null)
            {
                _context.VetReviews.Remove(existingReview);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your review has been permanently deleted.";
            }

            return RedirectToAction("Profile", new { id = vetId });
        }

        // --------------------------------------------------------
        // CREATE PROFILE (GET)
        // --------------------------------------------------------
        [Authorize(Roles = "Vet")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (await _context.VetProfiles.AnyAsync(p => p.UserAccountId == user.Id))
                return RedirectToAction(nameof(Edit));

            var vm = new VetProfileFormVM
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                bool isWeekend = day == DayOfWeek.Friday || day == DayOfWeek.Saturday;
                vm.Schedule.Add(new WorkingDayVM
                {
                    Day = day,
                    IsOff = isWeekend,
                    OpenTime = isWeekend ? null : "09:00",
                    CloseTime = isWeekend ? null : "17:00"
                });
            }

            ViewData["VetName"] = user.FullName ?? "Veterinarian";

            return View(vm);
        }

        // --------------------------------------------------------
        // CREATE PROFILE (POST)
        // --------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Vet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VetProfileFormVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var existingProfile = await _context.VetProfiles
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);
            if (existingProfile != null)
                return RedirectToAction("Profile", new { id = existingProfile.Id });

            if (user.Email?.ToLower() != vm.Email.ToLower() &&
                await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError("Email", "This email is already in use by another account.");
                return View(vm);
            }

            user.FullName = vm.FullName;
            user.Email = vm.Email;
            user.UserName = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            user.IsProfileComplete = true;
            await _userManager.UpdateAsync(user);

            var profile = new VetProfile
            {
                UserAccountId = user.Id,
                Specialization = vm.Specialization,
                ClinicName = vm.ClinicName,
                ClinicAddress = vm.ClinicAddress,
                YearsOfExperience = vm.YearsOfExperience,
                Description = vm.Description,
                Services = vm.Services,
                ImageUrl = await ProcessUploadedFile(vm.ImageFile) ?? "/images/default-vet.png"
            };

            foreach (var item in vm.Schedule)
            {
                profile.Schedule.Add(new WorkingDay
                {
                    Day = item.Day,
                    IsOff = item.IsOff,
                    OpenTime = (!item.IsOff && TimeSpan.TryParse(item.OpenTime, out var ot)) ? ot : null,
                    CloseTime = (!item.IsOff && TimeSpan.TryParse(item.CloseTime, out var ct)) ? ct : null
                });
            }

            _context.VetProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", new { id = profile.Id });
        }

        // --------------------------------------------------------
        // EDIT PROFILE (GET)
        // --------------------------------------------------------
        [Authorize(Roles = "Vet")]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.VetProfiles
                .Include(p => p.Schedule)
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return RedirectToAction(nameof(Create));

            if (profile.Schedule == null || !profile.Schedule.Any())
            {
                profile.Schedule = new List<WorkingDay>();
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newDay = new WorkingDay { Day = day, IsOff = true, VetProfileId = profile.Id };
                    _context.WorkingDays.Add(newDay);
                    profile.Schedule.Add(newDay);
                }
                await _context.SaveChangesAsync();
            }

            // Pass layout data
            ViewData["VetImageUrl"] = profile.ImageUrl;
            ViewData["VetName"] = user.FullName ?? "Veterinarian";

            var vm = new VetProfileFormVM
            {
                Id = profile.Id,
                FullName = user.FullName ?? string.Empty,
                ClinicName = profile.ClinicName,
                ClinicAddress = profile.ClinicAddress,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Specialization = profile.Specialization,
                YearsOfExperience = profile.YearsOfExperience,
                Description = profile.Description,
                Services = profile.Services ?? "",
                ExistingImageUrl = profile.ImageUrl,
                Schedule = profile.Schedule.Select(w => new WorkingDayVM
                {
                    Id = w.Id,
                    Day = w.Day,
                    IsOff = w.IsOff,
                    OpenTime = w.OpenTime?.ToString(@"hh\:mm"),
                    CloseTime = w.CloseTime?.ToString(@"hh\:mm")
                }).OrderBy(s => ((int)s.Day + 6) % 7).ToList()
            };

            return View(vm);
        }

        // --------------------------------------------------------
        // EDIT PROFILE (POST)
        // --------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Vet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VetProfileFormVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (user.Email?.ToLower() != vm.Email.ToLower() &&
                await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError("Email", "This email address is already in use.");
                return View(vm);
            }

            var profile = await _context.VetProfiles
                .Include(p => p.Schedule)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return NotFound();

            user.FullName = vm.FullName;
            user.Email = vm.Email;
            user.UserName = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            await _userManager.UpdateAsync(user);

            profile.ClinicName = vm.ClinicName;
            profile.ClinicAddress = vm.ClinicAddress;
            profile.Specialization = vm.Specialization;
            profile.YearsOfExperience = vm.YearsOfExperience;
            profile.Description = vm.Description;
            profile.Services = vm.Services;

            if (vm.ImageFile != null)
                profile.ImageUrl = await ProcessUploadedFile(vm.ImageFile);

            foreach (var item in vm.Schedule)
            {
                var dbDay = profile.Schedule.FirstOrDefault(w => w.Day == item.Day);
                if (dbDay != null)
                {
                    dbDay.IsOff = item.IsOff;
                    dbDay.OpenTime = (!item.IsOff && TimeSpan.TryParse(item.OpenTime, out var ot)) ? ot : null;
                    dbDay.CloseTime = (!item.IsOff && TimeSpan.TryParse(item.CloseTime, out var ct)) ? ct : null;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Profile", new { id = profile.Id });
        }

        // --------------------------------------------------------
        // DELETE PROFILE
        // --------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Vet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.VetProfiles
                .Include(p => p.Schedule)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile != null)
            {
                if (profile.Schedule?.Any() == true)
                    _context.RemoveRange(profile.Schedule);

                if (profile.Reviews?.Any() == true)
                    _context.RemoveRange(profile.Reviews);

                _context.VetProfiles.Remove(profile);

                user.IsProfileComplete = false;
                await _userManager.UpdateAsync(user);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Create", "Vet");
        }


        [HttpGet]
        public async Task<IActionResult> SearchClients(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>());

            var rawTerm = q.Trim();
            var searchTerm = $"%{rawTerm}%";

            string idSearchTerm = rawTerm;
            if (rawTerm.StartsWith("ACC-", StringComparison.OrdinalIgnoreCase))
            {
                idSearchTerm = rawTerm.Substring(4);
            }
            var idSearchPattern = $"{idSearchTerm}%";

            var vets = await _userManager.GetUsersInRoleAsync("Vet");
            var shelters = await _userManager.GetUsersInRoleAsync("Shelter");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            var excludedUserIds = vets.Concat(shelters).Concat(admins).Select(u => u.Id).ToList();

            var clients = await _context.ClientProfiles
                .Include(c => c.UserAccount)
                .Where(c => !excludedUserIds.Contains(c.UserAccountId)) 
                .Where(c =>
                    (c.UserAccount.FullName != null && EF.Functions.Like(c.UserAccount.FullName, searchTerm)) ||
                    (c.UserAccount.Email != null && EF.Functions.Like(c.UserAccount.Email, searchTerm)) ||
                    (c.UserAccount.PhoneNumber != null && EF.Functions.Like(c.UserAccount.PhoneNumber, searchTerm)) ||
                    (c.UserAccount.Id != null && EF.Functions.Like(c.UserAccount.Id, idSearchPattern))
                )
                .Take(50)
                .Select(c => new
                {
                    clientProfileId = c.Id,
                    name = c.UserAccount.FullName,
                    email = c.UserAccount.Email,
                    accountId = "ACC-" + c.UserAccount.Id.Substring(0, 4).ToUpper()
                })
                .ToListAsync();

            return Json(clients);
        }

        [HttpGet]
        public async Task<IActionResult> AddPatient(int? apptId)
        {
            var vm = new PetFormVM();

            // If they clicked "Verify Pet" from the Dashboard...
            if (apptId.HasValue)
            {
                var apt = await _context.Appointments
                    .Include(a => a.ClientProfile).ThenInclude(c => c.UserAccount)
                    .FirstOrDefaultAsync(a => a.Id == apptId);

                if (apt != null)
                {
                    vm.SelectedClientProfileId = apt.ClientProfileId;
                    ViewBag.PreselectedClientName = apt.ClientProfile.UserAccount?.FullName + " (" + apt.ClientProfile.UserAccount?.Email + ")";
                    ViewBag.PendingApptId = apptId.Value;

                    // Parse the new pet details out of the Notes field!
                    if (apt.Notes != null && apt.Notes.Contains("[NEW PATIENT VERIFICATION]"))
                    {
                        try
                        {
                            var nameSpan = apt.Notes.Substring(apt.Notes.IndexOf("Pet Name:") + 9);
                            var nameExtracted = nameSpan.Substring(0, nameSpan.IndexOf("|")).Trim();

                            var breedSpan = apt.Notes.Substring(apt.Notes.IndexOf("Breed:") + 6);
                            var breedExtracted = breedSpan.Substring(0, breedSpan.IndexOf("||")).Trim();

                            vm.Name = nameExtracted;
                            vm.Breed = breedExtracted;
                        }
                        catch { } // Failsafe if string parsing breaks
                    }
                }
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPatient(PetFormVM vm, int? pendingApptId)
        {
            if (vm.SelectedClientProfileId == null || vm.SelectedClientProfileId == 0)
            {
                ModelState.AddModelError("SelectedClientProfileId", "You must assign this patient to an official Client Profile.");
                return View(vm);
            }

            var pet = new Pet
            {
                ClientProfileId = vm.SelectedClientProfileId,
                ShelterProfileId = null,
                Name = vm.Name,
                Breed = vm.Breed,
                DateOfBirth = vm.DateOfBirth,
                Gender = vm.Gender,
                IsNeutered = vm.IsNeutered,
                Description = vm.Description ?? "",
                MedicalHistory = vm.MedicalHistory ?? "No prior medical history provided.",
                IsAdopted = true,
                IsPubliclyListed = false,
                ImageUrl = await ProcessUploadedFile(vm.ImageFile) ?? "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=400&q=80"
            };

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            // If we came from a pending dashboard request, map the pet and confirm the appointment instantly!
            if (pendingApptId.HasValue)
            {
                var apt = await _context.Appointments.FindAsync(pendingApptId.Value);
                if (apt != null)
                {
                    apt.PetId = pet.Id;
                    apt.Status = "Confirmed";
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Patient {pet.Name} registered, and their appointment has been confirmed!";
                    return RedirectToAction(nameof(Dashboard));
                }
            }

            TempData["SuccessMessage"] = $"Patient {pet.Name} successfully registered to client account!";
            return RedirectToAction(nameof(Dashboard));
        }

        // --------------------------------------------------------
        // DASHBOARD & HISTORY: SINGLE APPOINTMENT STATUS UPDATE
        // --------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Vet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, string status)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var vetProfile = await _context.VetProfiles.FirstOrDefaultAsync(p => p.UserAccountId == user.Id);
            if (vetProfile == null) return RedirectToAction("Dashboard");

            // Fetch the appointment, ensuring it actually belongs to this specific Vet
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.VetProfileId == vetProfile.Id);

            if (appointment != null)
            {
                appointment.Status = status;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment marked as completed!";
            }

            // This return works flawlessly with BOTH the background AJAX script and a normal page load
            return RedirectToAction(nameof(AppointmentHistory));
        }

        // --------------------------------------------------------
        // APPOINTMENT HISTORY (NEW)
        // --------------------------------------------------------
        [Authorize(Roles = "Vet")]
        public async Task<IActionResult> AppointmentHistory(string status = "All", string sortOrder = "desc")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.VetProfiles.FirstOrDefaultAsync(p => p.UserAccountId == user.Id);
            if (profile == null) return RedirectToAction("Create");

            ViewData["VetImageUrl"] = profile.ImageUrl;
            ViewData["VetName"] = user.FullName ?? "Veterinarian";

            // Base query
            var query = _context.Appointments
                .Include(a => a.ClientProfile).ThenInclude(c => c.UserAccount)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Where(a => a.VetProfileId == profile.Id);

            // 1. Apply Status Filter
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(a => a.Status == status);
            }

            // 2. Apply Sorting Filter
            if (sortOrder == "asc")
            {
                query = query.OrderBy(a => a.AppointmentDate); // Oldest/Closest first
            }
            else
            {
                query = query.OrderByDescending(a => a.AppointmentDate); // Newest/Furthest first
            }

            var appointments = await query.ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSort = sortOrder;

            return View(appointments);
        }
        // --------------------------------------------------------
        // HELPER: FILE UPLOAD
        // --------------------------------------------------------
        private async Task<string?> ProcessUploadedFile(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/vets");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            using var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create);
            await file.CopyToAsync(fileStream);

            return "/uploads/vets/" + uniqueFileName;
        }
    }
}