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
                Id = user.Id, // We use the REAL Account ID for the Vet Search
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

            // --- STRICT RULE: EMAIL UNIQUENESS CHECK ---
            // If they changed their email, make sure no one else is already using it!
            if (user.Email.ToLower() != vm.Email.ToLower())
            {
                var existingEmailUser = await _userManager.FindByEmailAsync(vm.Email);
                if (existingEmailUser != null && existingEmailUser.Id != user.Id)
                {
                    ModelState.AddModelError("Email", "This email address is already in use by another account.");
                    return View(vm);
                }
            }

            // Update core user details
            user.FullName = vm.FullName;
            user.Email = vm.Email;
            user.UserName = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            user.Gender = vm.Gender;
            user.City = vm.City;

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);

            // --- IMAGE UPLOAD LOGIC ---
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

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

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
                    LastNameChangeDate = p.LastNameChangeDate // Pulls the date from DB
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

            // STRICT RULE: 3-Month Name Change Check
            if (!string.IsNullOrWhiteSpace(vm.Name) && pet.Name != vm.Name)
            {
                if (pet.LastNameChangeDate.HasValue && (DateTime.Now - pet.LastNameChangeDate.Value).TotalDays < 90)
                {
                    TempData["ErrorMessage"] = $"Security Lock: You cannot change {pet.Name}'s name until {pet.LastNameChangeDate.Value.AddDays(90):MMM dd, yyyy}.";
                    return RedirectToAction(nameof(MyPets));
                }

                pet.Name = vm.Name;
                pet.LastNameChangeDate = DateTime.Now; // Starts the 90-day timer!
            }

            pet.Description = vm.Description;

            // Handle Profile Image Upload
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
        [Authorize] // Make sure only logged-in clients can delete
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePet(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 1. Find the client profile
            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            if (clientProfile == null) return NotFound();

            // 2. Find the pet, ensuring it actually belongs to this client!
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.ClientProfileId == clientProfile.Id);
            if (pet == null) return NotFound();

            // 3. Remove the pet from the database
            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{pet.Name} has been successfully removed from your profile.";

            // Redirect back to wherever your MyPets page lives (e.g., "MyPets" or "Dashboard")
            return RedirectToAction("MyPets");
        }

        // --------------------------------------------------------
        // APPOINTMENTS (REAL DATABASE DATA)
        // --------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Appointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // 1. Get the Client Profile
            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            if (clientProfile == null) return RedirectToAction("Dashboard");

            // 2. Fetch REAL appointments from the database for this client
            var allApts = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.VetProfile)
                .Where(a => a.ClientProfileId == clientProfile.Id) // Much safer: checking the Appointment's Client ID directly
                .Select(a => new AppointmentDisplayVM
                {
                    Id = a.Id,
                    // Safely handle nullable Pet
                    PetName = a.Pet != null ? a.Pet.Name : "My Pet",
                    PetImageUrl = (a.Pet != null && a.Pet.ImageUrl != null) ? a.Pet.ImageUrl : "https://placehold.co/400x400/FAF6F1/5C3D1E?text=Pet",

                    // THE FIX: Using Notes (or a default string) instead of Reason
                    Reason = string.IsNullOrWhiteSpace(a.Notes) ? (a.IsUrgent ? "Urgent Visit" : "Scheduled Visit") : a.Notes,

                    ClinicName = a.VetProfile.ClinicName,

                    // THE FIX: Using ClinicAddress instead of Location
                    ClinicAddress = a.VetProfile.ClinicAddress,

                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
                })
                .ToListAsync();

            // 3. Split them into Upcoming (excluding Cancelled) and Past
            var vm = new ClientAppointmentsVM
            {
                UpcomingAppointments = allApts
                    .Where(a => a.AppointmentDate >= DateTime.Now && a.Status != "Cancelled")
                    .OrderBy(a => a.AppointmentDate)
                    .ToList(),

                PastAppointments = allApts
                    .Where(a => a.AppointmentDate < DateTime.Now || a.Status == "Completed")
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToList()
            };

            return View(vm);
        }

        // --------------------------------------------------------
        // CANCEL APPOINTMENT (REAL DATABASE UPDATE)
        // --------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            if (clientProfile == null) return Unauthorized();

            // 1. Find the appointment and ensure it actually belongs to this client!
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ClientProfileId == clientProfile.Id);

            if (appointment != null)
            {
                // 2. Update the status in the database
                appointment.Status = "Cancelled";
                _context.Update(appointment);
                await _context.SaveChangesAsync();

                // Return OK so the Javascript knows it's safe to animate the card away
                return Ok();
            }

            return BadRequest("Appointment not found or access denied.");
        }
    }
}