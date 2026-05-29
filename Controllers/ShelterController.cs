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
    [Authorize]
    public class ShelterController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ShelterController(AppDbContext context, UserManager<UserAccount> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // ========================================================
        // AUTOMATED SYNC METHOD FOR BOARDING DATES
        // ========================================================
        private async Task SyncBoardingStatusesAsync(int shelterId)
        {
            bool modified = false;
            var now = DateTime.Now;

            // 1. DropOff -> Boarding (When real-time hits Drop-off time)
            var dropOffs = await _context.BoardingRecords
                .Where(b => b.ShelterProfileId == shelterId && b.Status == "DropOff" && b.ScheduledDate <= now)
                .ToListAsync();

            foreach (var b in dropOffs)
            {
                b.Status = "Boarding";
                b.TimeLabel = $"{b.ScheduledDate:hh:mm tt} • Boarding";

                // ADD to Internal Pets because boarding is now Active
                var linkedPet = await _context.Pets.FirstOrDefaultAsync(p => p.Name == b.PetName && p.ClientProfileId != null);
                if (linkedPet != null && linkedPet.ShelterProfileId != shelterId)
                {
                    linkedPet.ShelterProfileId = shelterId;
                }
                modified = true;
            }

            // 2. Boarding -> PickUp (When real-time hits Pick-up time)
            var readyForPickup = await _context.BoardingRecords
                .Where(b => b.ShelterProfileId == shelterId && b.Status == "Boarding" && b.PickUpDate.HasValue && b.PickUpDate.Value <= now)
                .ToListAsync();

            foreach (var b in readyForPickup)
            {
                b.Status = "PickUp";
                b.TimeLabel = $"{b.PickUpDate.Value:hh:mm tt} • Pick-up";

                // REMOVE from Internal Pets because they are waiting for pick-up
                var linkedPet = await _context.Pets.FirstOrDefaultAsync(p => p.Name == b.PetName && p.ClientProfileId != null);
                if (linkedPet != null && linkedPet.ShelterProfileId == shelterId)
                {
                    linkedPet.ShelterProfileId = null;
                }
                modified = true;
            }

            if (modified)
            {
                await _context.SaveChangesAsync();
            }
        }

        // --------------------------------------------------------
        // DASHBOARD
        // --------------------------------------------------------
        [HttpGet]
        [Authorize(Roles = "Shelter")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var shelter = await _context.ShelterProfiles.FirstOrDefaultAsync(s => s.UserAccountId == user.Id);
            if (shelter == null) return RedirectToAction(nameof(Create));

            int currentShelterId = shelter.Id;

            await SyncBoardingStatusesAsync(currentShelterId);

            var viewModel = new ShelterDashboardVM
            {
                ShelterName = shelter.ShelterName,
                ImageUrl = shelter.ImageUrl ?? "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(shelter.ShelterName) + "&background=FAF6F1&color=d9534f&bold=true",

                InResidenceCount = await _context.Pets.CountAsync(p => p.ShelterProfileId == currentShelterId && p.IsAdopted == false && p.ClientProfileId == null && p.IsPubliclyListed == true),
                PendingAdoptionsCount = await _context.AdoptionApplications.CountAsync(a => a.Pet.ShelterProfileId == currentShelterId && a.Type == "Adoption" && a.Status == "Pending"),
                IntakeRequestsCount = await _context.AdoptionApplications.CountAsync(a => a.Pet.ShelterProfileId == currentShelterId && a.Type == "Transfer" && a.Status == "Pending"),
                ActiveBoardingCount = await _context.BoardingRecords.CountAsync(b => b.ShelterProfileId == currentShelterId && b.Status != "Pending" && b.Status != "Completed")
            };

            var apps = await _context.AdoptionApplications
                .Include(a => a.Pet)
                .Include(a => a.ClientProfile).ThenInclude(c => c.UserAccount)
                .Where(a => a.Pet.ShelterProfileId == currentShelterId && (a.Status == "Pending" || a.Status == "Approved"))
                .Select(a => new QueueItemDto
                {
                    Id = a.Id,
                    Type = a.Type,
                    PetName = a.Pet.Name,
                    PetInfo = $"{a.Pet.Breed} • {a.Pet.Age}yrs",
                    PetImageUrl = a.Pet.ImageUrl ?? "https://placehold.co/100x100/EBE0D5/5C3D1E?text=Pet",
                    ApplicantName = a.ClientProfile.UserAccount.FullName,
                    ApplicantContact = a.ClientProfile.UserAccount.PhoneNumber ?? a.ClientProfile.UserAccount.Email,
                    Status = a.Status
                })
                .ToListAsync();

            var pendingBoardings = await _context.BoardingRecords
                .Where(b => b.ShelterProfileId == currentShelterId && b.Status == "Pending")
                .Select(b => new QueueItemDto
                {
                    Id = b.Id,
                    Type = "Boarding",
                    PetName = b.PetName,
                    PetInfo = $"{b.PetBreed} • Boarding Req.",
                    // FIXED: Removed the ShelterProfile check so it successfully pulls the client's pet image!
                    PetImageUrl = _context.Pets.Where(p => p.Name == b.PetName).Select(p => p.ImageUrl).FirstOrDefault() ?? "https://placehold.co/100x100/EBE0D5/5C3D1E?text=Pet",
                    ApplicantName = b.OwnerName,
                    ApplicantContact = "Client",
                    Status = b.Status
                })
                .ToListAsync();

            viewModel.QueueItems = apps.Concat(pendingBoardings).OrderByDescending(q => q.Id).ToList();

            viewModel.BoardingLogs = await _context.BoardingRecords
                .Where(b => b.ShelterProfileId == currentShelterId
                         && b.Status != "Completed" && b.Status != "Pending")
                .OrderBy(b => b.ScheduledDate)
                .Select(b => new BoardingLogDto
                {
                    Id = b.Id,
                    PetName = b.PetName,
                    PetBreed = b.PetBreed,
                    OwnerName = b.OwnerName,
                    TimeLabel = b.TimeLabel,
                    StatusType = b.Status,
                    SpecialNotes = b.SpecialNotes,
                    ScheduledDate = b.ScheduledDate,
                    PickUpDate = b.PickUpDate
                })
                .ToListAsync();

            return View(viewModel);
        }

        // --------------------------------------------------------
        // PUBLIC PROFILE VIEW
        // --------------------------------------------------------
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Profile(int? id)
        {
            ShelterProfile? shelter = null;

            if (id.HasValue && id.Value > 0)
            {
                shelter = await _context.ShelterProfiles
                    .Include(s => s.UserAccount)
                    .Include(s => s.Schedule)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            else if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    shelter = await _context.ShelterProfiles
                        .Include(s => s.UserAccount)
                        .Include(s => s.Schedule)
                        .FirstOrDefaultAsync(s => s.UserAccountId == user.Id);
                }
            }

            if (shelter == null) return NotFound();

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

            var viewModel = new ShelterProfileVM
            {
                Id = shelter.Id,
                UserAccountId = shelter.UserAccountId,
                FullName = shelter.UserAccount?.FullName ?? "Shelter Manager",
                ShelterName = shelter.ShelterName,
                Description = shelter.Description ?? "We are dedicated to rescuing, rehabilitating, and finding loving, forever homes for animals in need.",
                Address = shelter.Address ?? "123 Rescue Lane, Amman, Jordan",
                ImageUrl = shelter.ImageUrl,
                Email = shelter.UserAccount?.Email ?? "contact@shelter.com",
                PhoneNumber = shelter.UserAccount?.PhoneNumber ?? "(555) 123-4567",
                Schedule = shelter.Schedule?.ToList() ?? new List<WorkingDay>(),

                AvailablePetsCount = await _context.Pets.CountAsync(p => p.ShelterProfileId == shelter.Id && p.IsAdopted == false && p.IsPubliclyListed == true && p.ClientProfileId == null),
                TotalAdoptions = await _context.Pets.CountAsync(p => p.ShelterProfileId == shelter.Id && p.IsAdopted == true),

                AvailablePets = await _context.Pets
                    .Where(p => p.ShelterProfileId == shelter.Id && p.IsAdopted == false && p.IsPubliclyListed == true && p.ClientProfileId == null)
                    .Select(p => new ShelterPetDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Breed = p.Breed,
                        Age = DateTime.Now.Year - p.DateOfBirth.Year,
                        ImageUrl = p.ImageUrl ?? "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=400&q=80",
                        Gender = p.Gender.ToString(),
                        IsNeutered = p.IsNeutered,
                        MedicalHistory = p.MedicalHistory ?? "No known medical issues.",
                        Description = p.Description ?? "A lovely companion looking for a home."
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // --------------------------------------------------------
        // BOARDING MANAGEMENT (UNIFIED CLIENT & SHELTER SUBMISSION)
        // --------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBoardingSession(int? shelterId, List<int>? petIds, string PetName, string PetBreed, string OwnerName, string ScheduledDate, string PickUpDate, string Status, string SpecialNotes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // FIXED: Securely parse string dates to avoid model binding failures
            DateTime parsedSchedule = DateTime.Now;
            if (DateTime.TryParse(ScheduledDate, out DateTime ps)) parsedSchedule = ps;

            DateTime? parsedPickUp = null;
            if (!string.IsNullOrWhiteSpace(PickUpDate) && DateTime.TryParse(PickUpDate, out DateTime pu)) parsedPickUp = pu;

            int targetShelterId = shelterId ?? 0;
            bool isClientSubmission = shelterId.HasValue;

            if (targetShelterId == 0)
            {
                var shelter = await _context.ShelterProfiles.FirstOrDefaultAsync(s => s.UserAccountId == user.Id);
                if (shelter != null) targetShelterId = shelter.Id;
            }

            var shelterProfile = await _context.ShelterProfiles.FindAsync(targetShelterId);
            if (shelterProfile == null) return NotFound();

            var dName = parsedSchedule.DayOfWeek;
            var schedule = await _context.WorkingDays.FirstOrDefaultAsync(w => w.ShelterProfileId == targetShelterId && w.Day == dName);

            if (schedule == null || schedule.IsOff || parsedSchedule.TimeOfDay < schedule.OpenTime || parsedSchedule.TimeOfDay > schedule.CloseTime)
            {
                TempData["ErrorMessage"] = "Booking Failed: Selected time is outside of the shelter's working hours.";
                if (isClientSubmission) return RedirectToAction("Profile", new { id = shelterId });
                return RedirectToAction("Dashboard");
            }

            string finalStatus = isClientSubmission ? "Pending" : Status;
            string formattedTime = parsedSchedule.ToString("hh:mm tt");
            string actionLabel = finalStatus == "Pending" ? "Pending Approval" : (finalStatus == "DropOff" ? "Drop-off" : (finalStatus == "PickUp" ? "Pick-up" : "Boarding"));
            string fullTimeLabel = isClientSubmission ? "Awaiting Review" : $"{formattedTime} • {actionLabel}";

            if (petIds != null && petIds.Any())
            {
                foreach (var pid in petIds)
                {
                    var pet = await _context.Pets.FindAsync(pid);
                    if (pet != null)
                    {
                        var newBoarding = new BoardingRecord
                        {
                            ShelterProfileId = targetShelterId,
                            PetName = pet.Name,
                            PetBreed = pet.Breed,
                            OwnerName = OwnerName,
                            ScheduledDate = parsedSchedule,
                            PickUpDate = parsedPickUp,
                            Status = finalStatus,
                            TimeLabel = fullTimeLabel,
                            SpecialNotes = SpecialNotes
                        };
                        _context.BoardingRecords.Add(newBoarding);
                    }
                }
            }
            else
            {
                var newBoarding = new BoardingRecord
                {
                    ShelterProfileId = targetShelterId,
                    PetName = PetName,
                    PetBreed = PetBreed,
                    OwnerName = string.IsNullOrWhiteSpace(OwnerName) ? "-" : OwnerName,
                    ScheduledDate = parsedSchedule,
                    PickUpDate = parsedPickUp,
                    Status = finalStatus,
                    TimeLabel = fullTimeLabel,
                    SpecialNotes = string.IsNullOrWhiteSpace(SpecialNotes) ? "Standard" : SpecialNotes
                };
                _context.BoardingRecords.Add(newBoarding);
            }

            await _context.SaveChangesAsync();

            if (isClientSubmission)
            {
                TempData["SuccessMessage"] = "Your boarding request has been sent to the shelter for approval!";
                return RedirectToAction("Profile", new { id = targetShelterId });
            }

            TempData["SuccessMessage"] = "Boarding session scheduled successfully!";
            return RedirectToAction("Dashboard");
        }

        // --------------------------------------------------------
        // DASHBOARD: QUEUE MANAGEMENT (ACCEPT / REJECT / FINALIZE)
        // --------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBoarding(int id)
        {
            var session = await _context.BoardingRecords.FindAsync(id);
            if (session != null && session.Status == "Pending")
            {
                if (session.ScheduledDate <= DateTime.Now)
                {
                    session.Status = "Boarding";
                    session.TimeLabel = $"{session.ScheduledDate:hh:mm tt} • Boarding";

                    var linkedPet = await _context.Pets.FirstOrDefaultAsync(p => p.Name == session.PetName && p.ClientProfileId != null);
                    if (linkedPet != null) linkedPet.ShelterProfileId = session.ShelterProfileId;
                }
                else
                {
                    session.Status = "DropOff";
                    session.TimeLabel = $"{session.ScheduledDate:hh:mm tt} • Drop-off";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Boarding request for {session.PetName} approved!";
            }
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineBoarding(int id)
        {
            var session = await _context.BoardingRecords.FindAsync(id);
            if (session != null && session.Status == "Pending")
            {
                var linkedPet = await _context.Pets.FirstOrDefaultAsync(p => p.Name == session.PetName && p.ShelterProfileId == session.ShelterProfileId && p.ClientProfileId != null);
                if (linkedPet != null)
                {
                    linkedPet.ShelterProfileId = null;
                }

                _context.BoardingRecords.Remove(session);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Boarding request declined.";
            }
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessTransfer(int id)
        {
            var app = await _context.AdoptionApplications.Include(a => a.Pet).FirstOrDefaultAsync(a => a.Id == id);
            if (app != null && app.Type == "Transfer")
            {
                app.Pet.ClientProfileId = null;
                app.Pet.IsAdopted = false;
                app.Pet.IsPubliclyListed = false;
                app.Status = "Completed";

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Intake successful! {app.Pet.Name} is now in your internal residents list.";
            }
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAdoption(int id)
        {
            var app = await _context.AdoptionApplications.FirstOrDefaultAsync(a => a.Id == id);
            if (app != null)
            {
                app.Status = "Approved";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Adoption application approved! Awaiting finalization.";
            }
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineApplication(int id)
        {
            var app = await _context.AdoptionApplications.Include(a => a.Pet).FirstOrDefaultAsync(a => a.Id == id);
            if (app != null)
            {
                app.Status = "Declined";

                if (app.Type == "Transfer")
                {
                    app.Pet.ShelterProfileId = null;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Request declined successfully.";
            }
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeAdoption(int id)
        {
            var app = await _context.AdoptionApplications.Include(a => a.Pet).FirstOrDefaultAsync(a => a.Id == id);
            if (app != null && app.Type == "Adoption")
            {
                app.Pet.ClientProfileId = app.ClientProfileId;
                app.Pet.ShelterProfileId = null;
                app.Pet.IsAdopted = true;
                app.Status = "Completed";

                var otherApps = await _context.AdoptionApplications
                                    .Where(a => a.PetId == app.PetId && a.Id != app.Id && a.Status == "Pending")
                                    .ToListAsync();

                foreach (var otherApp in otherApps)
                {
                    otherApp.Status = "Declined";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Adoption finalized! {app.Pet.Name} has officially been transferred to their new owner.";
            }
            return RedirectToAction(nameof(Dashboard));
        }

        // --------------------------------------------------------
        // INTERNAL DASHBOARD LOG METHODS
        // --------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBoardingSession(int id, string PetName, string PetBreed, string OwnerName, string ScheduledDate, string PickUpDate, string Status, string SpecialNotes)
        {
            var session = await _context.BoardingRecords.FindAsync(id);
            if (session != null)
            {
                session.PetName = PetName;
                session.PetBreed = PetBreed;
                session.OwnerName = string.IsNullOrWhiteSpace(OwnerName) ? "-" : OwnerName;

                // FIXED: Manually parsing the dates prevents ASP.NET from failing and returning 0001-01-01
                if (DateTime.TryParse(ScheduledDate, out DateTime ps)) session.ScheduledDate = ps;

                if (!string.IsNullOrWhiteSpace(PickUpDate) && DateTime.TryParse(PickUpDate, out DateTime pu)) session.PickUpDate = pu;
                else session.PickUpDate = null;

                session.Status = Status;

                DateTime displayDate = (Status == "PickUp" && session.PickUpDate.HasValue) ? session.PickUpDate.Value : session.ScheduledDate;
                string formattedTime = displayDate.ToString("hh:mm tt");
                string actionLabel = Status == "DropOff" ? "Drop-off" : (Status == "PickUp" ? "Pick-up" : (Status == "Completed" ? "Completed" : "Boarding"));

                session.TimeLabel = $"{formattedTime} • {actionLabel}";
                session.SpecialNotes = string.IsNullOrWhiteSpace(SpecialNotes) ? "Standard" : SpecialNotes;

                // STRICT MANUAL INTERNAL PETS LOGIC
                var linkedPet = await _context.Pets.FirstOrDefaultAsync(p => p.Name == PetName && p.ClientProfileId != null);
                if (linkedPet != null)
                {
                    if (Status == "Boarding")
                    {
                        linkedPet.ShelterProfileId = session.ShelterProfileId;
                    }
                    else
                    {
                        if (linkedPet.ShelterProfileId == session.ShelterProfileId)
                        {
                            linkedPet.ShelterProfileId = null;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Boarding session updated successfully.";
            }
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBoardingSession(int id)
        {
            var session = await _context.BoardingRecords.FindAsync(id);
            if (session != null)
            {
                var linkedPet = await _context.Pets.FirstOrDefaultAsync(p => p.Name == session.PetName && p.ShelterProfileId == session.ShelterProfileId && p.ClientProfileId != null);

                if (linkedPet != null)
                {
                    linkedPet.ShelterProfileId = null;
                }

                _context.BoardingRecords.Remove(session);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Boarding session completed and removed successfully.";
            }
            return RedirectToAction(nameof(Dashboard));
        }

        // --------------------------------------------------------
        // INTAKE SURRENDER REQUEST (CLIENT SUBMISSION)
        // --------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitIntakeRequest(int shelterId, int petId, DateTime scheduledDate, string reason)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var client = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.ClientProfileId == client.Id);

            if (pet == null || client == null) return NotFound();

            var schedule = await _context.WorkingDays.FirstOrDefaultAsync(w => w.ShelterProfileId == shelterId && w.Day == scheduledDate.DayOfWeek);
            if (schedule == null || schedule.IsOff || scheduledDate.TimeOfDay < schedule.OpenTime || scheduledDate.TimeOfDay > schedule.CloseTime)
            {
                TempData["ErrorMessage"] = "Booking Failed: Selected time is outside of the shelter's working hours.";
                return RedirectToAction("Profile", new { id = shelterId });
            }

            pet.ShelterProfileId = shelterId;

            var app = new AdoptionApplication
            {
                PetId = pet.Id,
                ClientProfileId = client.Id,
                Type = "Transfer",
                Status = "Pending",
                ApplicationDate = scheduledDate
            };

            _context.AdoptionApplications.Add(app);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your intake surrender request has been submitted and is pending shelter approval.";
            return RedirectToAction("Profile", new { id = shelterId });
        }

        // --------------------------------------------------------
        // ADOPTION REQUEST (CLIENT SUBMISSION)
        // --------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAdoptionRequest(int PetId, bool AgreedToPolicy)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var client = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            if (client == null)
            {
                client = new ClientProfile { UserAccountId = user.Id };
                _context.ClientProfiles.Add(client);
                await _context.SaveChangesAsync();
            }

            var pet = await _context.Pets.FindAsync(PetId);
            if (pet == null) return NotFound();

            if (!AgreedToPolicy)
            {
                TempData["ErrorMessage"] = "You must agree to the shelter's post-adoption policy.";
                return RedirectToAction("Profile", new { id = pet.ShelterProfileId });
            }

            var app = new AdoptionApplication
            {
                PetId = PetId,
                ClientProfileId = client.Id,
                Type = "Adoption",
                Status = "Pending",
                ApplicationDate = DateTime.Now
            };

            _context.AdoptionApplications.Add(app);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Your adoption application for {pet.Name} has been submitted successfully!";

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && referer.Contains("/Shelter/Pets"))
            {
                return RedirectToAction("Pets", new { shelterId = pet.ShelterProfileId });
            }

            return RedirectToAction("Profile", new { id = pet.ShelterProfileId });
        }

        // --------------------------------------------------------
        // VIEW: INTERNAL / BOARDING PETS (SHELTER ONLY)
        // --------------------------------------------------------
        [HttpGet]
        [Authorize(Roles = "Shelter")]
        public async Task<IActionResult> InternalPets(int page = 1)
        {
            int pageSize = 12;
            var shelterUser = await _userManager.GetUserAsync(User);
            var shelter = await _context.ShelterProfiles.FirstOrDefaultAsync(s => s.UserAccountId == shelterUser.Id);

            if (shelter == null) return RedirectToAction("Dashboard");

            await SyncBoardingStatusesAsync(shelter.Id);

            var query = _context.Pets
                .Include(p => p.ClientProfile).ThenInclude(c => c.UserAccount)
                .Where(p => p.ShelterProfileId == shelter.Id &&
                            (p.IsPubliclyListed == false || p.ClientProfileId != null));

            var petsList = await query.ToListAsync();

            var ownerMap = new Dictionary<int, string>();
            var hasOwnerMap = new Dictionary<int, bool>();
            var publiclyListedMap = new Dictionary<int, bool>();

            foreach (var p in petsList)
            {
                if (p.ClientProfileId != null)
                {
                    ownerMap[p.Id] = "Boarding (Owner: " + p.ClientProfile.UserAccount.FullName + ")";
                    hasOwnerMap[p.Id] = true;
                }
                else
                {
                    ownerMap[p.Id] = p.IsPubliclyListed ? "Shelter Resident (Listed for Adoption)" : "Shelter Resident (Not Listed)";
                    hasOwnerMap[p.Id] = false;
                }
                publiclyListedMap[p.Id] = p.IsPubliclyListed;
            }

            ViewBag.OwnerMap = ownerMap;
            ViewBag.HasOwnerMap = hasOwnerMap;
            ViewBag.PubliclyListedMap = publiclyListedMap;

            var pets = petsList
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new MyPetDisplayVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Breed = p.Breed,
                    DateOfBirth = p.DateOfBirth,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    MedicalHistory = p.MedicalHistory,
                    Gender = p.Gender.ToString(),
                    IsNeutered = p.IsNeutered
                })
                .ToList();

            return View(pets);
        }

        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdoptability(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var shelter = await _context.ShelterProfiles.FirstOrDefaultAsync(s => s.UserAccountId == user.Id);
            if (shelter == null) return Unauthorized();

            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.ShelterProfileId == shelter.Id);
            if (pet == null) return NotFound();

            if (pet.ClientProfileId != null)
            {
                TempData["ErrorMessage"] = "You cannot list a client's boarded pet for public adoption.";
                return RedirectToAction(nameof(InternalPets));
            }

            pet.IsPubliclyListed = !pet.IsPubliclyListed;
            await _context.SaveChangesAsync();

            string status = pet.IsPubliclyListed ? "listed for adoption" : "removed from the public adoption catalog";
            TempData["SuccessMessage"] = $"{pet.Name} has been {status}.";

            return RedirectToAction(nameof(InternalPets));
        }

        // --------------------------------------------------------
        // SHELTER PROFILE MANAGEMENT
        // --------------------------------------------------------
        [HttpGet]
        [Authorize(Roles = "Shelter")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (await _context.ShelterProfiles.AnyAsync(p => p.UserAccountId == user.Id))
                return RedirectToAction(nameof(Edit));

            var vm = new ShelterProfileFormVM
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
                    OpenTime = isWeekend ? null : "10:00",
                    CloseTime = isWeekend ? null : "18:00"
                });
            }

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShelterProfileFormVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var existingProfile = await _context.ShelterProfiles
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);
            if (existingProfile != null)
                return RedirectToAction(nameof(Dashboard));

            user.FullName = vm.FullName;
            user.Email = vm.Email;
            user.UserName = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            user.IsProfileComplete = true;

            await _userManager.SetPhoneNumberAsync(user, vm.PhoneNumber);
            await _userManager.UpdateAsync(user);

            var profile = new ShelterProfile
            {
                UserAccountId = user.Id,
                ShelterName = vm.ShelterName,
                Address = vm.Address,
                Description = vm.Description,
                ImageUrl = await ProcessUploadedFile(vm.ImageFile) ?? "/images/default-shelter.png"
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

            _context.ShelterProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        [Authorize(Roles = "Shelter")]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.ShelterProfiles
                .Include(p => p.Schedule)
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return RedirectToAction(nameof(Create));

            if (profile.Schedule == null || !profile.Schedule.Any())
            {
                profile.Schedule = new List<WorkingDay>();
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newDay = new WorkingDay { Day = day, IsOff = true, ShelterProfileId = profile.Id };
                    _context.WorkingDays.Add(newDay);
                    profile.Schedule.Add(newDay);
                }
                await _context.SaveChangesAsync();
            }

            var vm = new ShelterProfileFormVM
            {
                Id = profile.Id,
                FullName = user.FullName ?? string.Empty,
                ShelterName = profile.ShelterName,
                Address = profile.Address,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Description = profile.Description,
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

        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShelterProfileFormVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _context.ShelterProfiles
                .Include(p => p.Schedule)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return NotFound();

            user.FullName = vm.FullName;
            user.Email = vm.Email;
            user.UserName = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;

            await _userManager.SetPhoneNumberAsync(user, vm.PhoneNumber);
            await _userManager.UpdateAsync(user);

            profile.ShelterName = vm.ShelterName;
            profile.Address = vm.Address;
            profile.Description = vm.Description;

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

        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.ShelterProfiles
                .Include(p => p.Schedule)
                .Include(p => p.Pets)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile != null)
            {
                if (profile.Schedule?.Any() == true)
                    _context.RemoveRange(profile.Schedule);

                var boardingRecords = await _context.BoardingRecords.Where(b => b.ShelterProfileId == profile.Id).ToListAsync();
                if (boardingRecords.Any())
                    _context.RemoveRange(boardingRecords);

                _context.ShelterProfiles.Remove(profile);

                user.IsProfileComplete = false;
                await _userManager.UpdateAsync(user);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Create", "Shelter");
        }

        [HttpGet]
        [Authorize(Roles = "Shelter")]
        public IActionResult AddPet()
        {
            return View(new PetFormVM());
        }

        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPet(PetFormVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            var shelter = await _context.ShelterProfiles.FirstOrDefaultAsync(s => s.UserAccountId == user.Id);
            if (shelter == null) return RedirectToAction("Create");

            var pet = new Pet
            {
                ShelterProfileId = shelter.Id,
                ClientProfileId = null,
                Name = vm.Name,
                Breed = vm.Breed,
                DateOfBirth = vm.DateOfBirth,
                Gender = vm.Gender,
                IsNeutered = vm.IsNeutered,

                Description = string.IsNullOrWhiteSpace(vm.Description) ? "No description provided." : vm.Description,
                MedicalHistory = string.IsNullOrWhiteSpace(vm.MedicalHistory) ? "No medical history recorded." : vm.MedicalHistory,

                IsAdopted = false,
                IsPubliclyListed = false,

                ImageUrl = await ProcessUploadedFile(vm.ImageFile) ?? "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=400&q=80"
            };

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{pet.Name} has been officially added to your shelter!";
            return RedirectToAction(nameof(Dashboard));
        }

        // --------------------------------------------------------
        // VIEW SHELTER PETS (PUBLIC & PRIVATE)
        // --------------------------------------------------------
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Pets(int? shelterId, int page = 1)
        {
            int pageSize = 8;
            int targetShelterId = 0;

            if (shelterId.HasValue && shelterId.Value > 0)
            {
                targetShelterId = shelterId.Value;
            }
            else if (User.Identity?.IsAuthenticated == true && User.IsInRole("Shelter"))
            {
                var shelterUser = await _userManager.GetUserAsync(User);
                var shelter = await _context.ShelterProfiles.FirstOrDefaultAsync(s => s.UserAccountId == shelterUser.Id);
                if (shelter != null) targetShelterId = shelter.Id;
            }

            if (targetShelterId == 0) return RedirectToAction("Main", "Home");

            var query = _context.Pets
                .Where(p => p.ShelterProfileId == targetShelterId && p.IsAdopted == false && p.IsPubliclyListed == true && p.ClientProfileId == null);

            var pets = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new MyPetDisplayVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Breed = p.Breed,
                    DateOfBirth = p.DateOfBirth,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    MedicalHistory = p.MedicalHistory,
                    Gender = p.Gender.ToString(),
                    IsNeutered = p.IsNeutered
                })
                .ToListAsync();

            return View(pets);
        }

        // --------------------------------------------------------
        // EDIT SHELTER PET
        // --------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateShelterPet(int PetId, string Name, string Description, string MedicalHistory, bool IsNeutered, IFormFile? ImageFile, string ExistingImageUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            var shelter = await _context.ShelterProfiles.FirstOrDefaultAsync(s => s.UserAccountId == user.Id);

            if (shelter == null) return Unauthorized();

            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == PetId && p.ShelterProfileId == shelter.Id);
            if (pet == null) return NotFound();

            pet.Name = Name;
            pet.IsNeutered = IsNeutered;

            pet.Description = string.IsNullOrWhiteSpace(Description) ? "No description provided." : Description;
            pet.MedicalHistory = string.IsNullOrWhiteSpace(MedicalHistory) ? "No medical history recorded." : MedicalHistory;

            if (ImageFile != null)
            {
                pet.ImageUrl = await ProcessUploadedFile(ImageFile);
            }
            else
            {
                pet.ImageUrl = ExistingImageUrl;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{pet.Name}'s profile has been updated successfully!";

            var referer = Request.Headers["Referer"].ToString();
            if (referer.Contains("InternalPets"))
            {
                return RedirectToAction("InternalPets");
            }

            return RedirectToAction("Pets");
        }

        // --------------------------------------------------------
        // DELETE SHELTER PET
        // --------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteShelterPet(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var shelter = await _context.ShelterProfiles.FirstOrDefaultAsync(s => s.UserAccountId == user.Id);
            if (shelter == null) return Unauthorized();

            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.ShelterProfileId == shelter.Id);
            if (pet == null) return NotFound();

            if (pet.ClientProfileId != null)
            {
                TempData["ErrorMessage"] = "You cannot delete a pet that belongs to a client. The client must remove it from their own account.";
                return RedirectToAction("InternalPets");
            }

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{pet.Name} has been permanently removed from the shelter database.";
            return RedirectToAction("InternalPets");
        }

        private async Task<string?> ProcessUploadedFile(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/shelters");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            using var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create);
            await file.CopyToAsync(fileStream);

            return "/uploads/shelters/" + uniqueFileName;
        }
    }
}