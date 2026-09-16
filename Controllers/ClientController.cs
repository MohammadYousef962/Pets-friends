using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Data.ViewModels;
using Pets_friends.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Pets_friends.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ClientController(AppDbContext context, UserManager<UserAccount> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            if (clientProfile == null)
            {
                clientProfile = new ClientProfile { UserAccountId = user.Id };
                _context.ClientProfiles.Add(clientProfile);
                await _context.SaveChangesAsync();
            }

            var recentPets = await _context.Pets
                .Where(p => p.ClientProfileId == clientProfile.Id)
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToListAsync();

            var vm = new ClientDashboardVM
            {
                Id = user.Id,
                FullName = user.FullName ?? "Member",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Gender = user.Gender ?? "",
                City = user.City ?? "",
                ExistingImageUrl = clientProfile.ImageUrl,
                RecentPets = recentPets
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);

            var vm = new ClientDashboardVM
            {
                Id = user.Id,
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Gender = user.Gender ?? "",
                City = user.City ?? "",
                ExistingImageUrl = clientProfile?.ImageUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ClientDashboardVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (user.Email.ToLower() != vm.Email.ToLower())
            {
                var existingEmailUser = await _userManager.FindByEmailAsync(vm.Email);
                if (existingEmailUser != null && existingEmailUser.Id != user.Id)
                {
                    ModelState.AddModelError("Email", "This email address is already in use by another account.");
                    return View(vm);
                }
            }

            user.FullName = vm.FullName;
            user.Email = vm.Email;
            user.UserName = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            user.Gender = vm.Gender;
            user.City = vm.City;

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);

            if (vm.ImageFile != null && clientProfile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(vm.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(fileStream);
                }

                clientProfile.ImageUrl = "/images/profiles/" + uniqueFileName;
                _context.Update(clientProfile);
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Dashboard));
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> MyPets()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            if (clientProfile == null) return RedirectToAction("Dashboard");

            var pets = await _context.Pets
                .Where(p => p.ClientProfileId == clientProfile.Id)
                .Select(p => new MyPetDisplayVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Breed = p.Breed,
                    ImageUrl = p.ImageUrl ?? "https://placehold.co/400x400/FAF6F1/5C3D1E?text=Pet",
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    IsNeutered = p.IsNeutered,
                    MedicalHistory = p.MedicalHistory,
                    Description = p.Description,
                    LastNameChangeDate = p.LastNameChangeDate
                })
                .ToListAsync();

            return View(pets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPet(EditClientPetVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == vm.PetId && p.ClientProfileId == clientProfile.Id);

            if (pet == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(vm.Name) && pet.Name != vm.Name)
            {
                if (pet.LastNameChangeDate.HasValue && (DateTime.Now - pet.LastNameChangeDate.Value).TotalDays < 90)
                {
                    TempData["ErrorMessage"] = $"Security Lock: You cannot change {pet.Name}'s name until {pet.LastNameChangeDate.Value.AddDays(90):MMM dd, yyyy}.";
                    return RedirectToAction(nameof(MyPets));
                }

                pet.Name = vm.Name;
                pet.LastNameChangeDate = DateTime.Now;
            }

            pet.Description = vm.Description;

            if (vm.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "pets");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(vm.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(fileStream);
                }

                pet.ImageUrl = "/images/pets/" + uniqueFileName;
            }

            _context.Update(pet);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{pet.Name}'s profile updated successfully!";
            return RedirectToAction(nameof(MyPets));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePet(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            if (clientProfile == null) return NotFound();

            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.ClientProfileId == clientProfile.Id);
            if (pet == null) return NotFound();

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{pet.Name} has been successfully removed from your profile.";
            return RedirectToAction("MyPets");
        }

        // ========================================================
        // APPOINTMENTS & DASHBOARD REQUESTS (UPDATED)
        // ========================================================
        [HttpGet]
        public async Task<IActionResult> Appointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            if (clientProfile == null) return RedirectToAction("Dashboard");

            // 1. VET APPOINTMENTS
            var allApts = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.VetProfile)
                .Where(a => a.ClientProfileId == clientProfile.Id)
                .Select(a => new AppointmentDisplayVM
                {
                    Id = a.Id,
                    PetName = a.Pet != null ? a.Pet.Name : "My Pet",
                    PetImageUrl = (a.Pet != null && a.Pet.ImageUrl != null) ? a.Pet.ImageUrl : "https://placehold.co/400x400/FAF6F1/5C3D1E?text=Pet",
                    Reason = string.IsNullOrWhiteSpace(a.Notes) ? (a.IsUrgent ? "Urgent Visit" : "Scheduled Visit") : a.Notes,
                    ClinicName = a.VetProfile.ClinicName,
                    ClinicAddress = a.VetProfile.ClinicAddress,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
                })
                .ToListAsync();

            // 2. CLIENT'S PETS (Used to map Boarding Sessions by pet name)
            var myPets = await _context.Pets.Where(p => p.ClientProfileId == clientProfile.Id).ToListAsync();
            var myPetNames = myPets.Select(p => p.Name).ToList();

            // 3. BOARDING SESSIONS
            var rawBoardingRecords = await _context.BoardingRecords.Where(b => b.OwnerName == user.FullName || myPetNames.Contains(b.PetName)).ToListAsync();
            var shelters = await _context.ShelterProfiles.ToListAsync();

            var boardingSessions = rawBoardingRecords.Select(b => new ClientBoardingDto
            {
                Id = b.Id,
                PetName = b.PetName,
                PetImageUrl = myPets.FirstOrDefault(p => p.Name == b.PetName)?.ImageUrl ?? "https://placehold.co/400x400/FAF6F1/5C3D1E?text=Pet",
                ShelterName = shelters.FirstOrDefault(s => s.Id == b.ShelterProfileId)?.ShelterName ?? "Unknown Shelter",
                ScheduledDate = b.ScheduledDate,
                PickUpDate = b.PickUpDate,
                Status = b.Status
            }).ToList();

            // 4. ADOPTION & SURRENDER APPLICATIONS
            // FIX: Pull data from the DB first using ToListAsync(), then project it with Select()
            var rawAdoptions = await _context.AdoptionApplications
                .Include(a => a.Pet)
                .Where(a => a.ClientProfileId == clientProfile.Id)
                .ToListAsync();

            var adoptions = rawAdoptions.Select(a => new ClientAdoptionDto
            {
                Id = a.Id,
                PetName = a.Pet.Name,
                PetImageUrl = a.Pet.ImageUrl ?? "https://placehold.co/400x400/FAF6F1/5C3D1E?text=Pet",
                Type = a.Type,
                ShelterName = shelters.FirstOrDefault(s => s.Id == a.Pet.ShelterProfileId)?.ShelterName ?? "Shelter",
                ApplicationDate = a.ApplicationDate,
                Status = a.Status
            }).ToList();

            var vm = new ClientAppointmentsVM
            {
                // Hide cancelled/completed from Upcoming
                UpcomingAppointments = allApts
                        .Where(a => a.AppointmentDate >= DateTime.Now && a.Status != "Cancelled" && a.Status != "Completed")
                        .OrderBy(a => a.AppointmentDate)
                        .ToList(),

                // Push all cancelled/completed to Past History, even if the date is in the future
                PastAppointments = allApts
                        .Where(a => a.AppointmentDate < DateTime.Now || a.Status == "Completed" || a.Status == "Cancelled")
                        .OrderByDescending(a => a.AppointmentDate)
                        .ToList(),

                BoardingSessions = boardingSessions.OrderByDescending(b => b.ScheduledDate).ToList(),
                AdoptionApplications = adoptions.OrderByDescending(a => a.ApplicationDate).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId && a.ClientProfileId == clientProfile.Id);

            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Access denied.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBoarding(int sessionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Only allow removing pending ones before drop-off
            var session = await _context.BoardingRecords.FindAsync(sessionId);
            if (session != null && session.Status == "Pending")
            {
                _context.BoardingRecords.Remove(session);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Cannot cancel active boarding.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAdoption(int applicationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            var application = await _context.AdoptionApplications.FirstOrDefaultAsync(a => a.Id == applicationId && a.ClientProfileId == clientProfile.Id);

            if (application != null && application.Status == "Pending")
            {
                _context.AdoptionApplications.Remove(application);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest("Cannot cancel processed applications.");
        }
    }
}