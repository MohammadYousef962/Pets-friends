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

        // --------------------------------------------------------
        // DASHBOARD
        // --------------------------------------------------------
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

            var pendingAppointments = await _context.Appointments
                .Include(a => a.ClientProfile).ThenInclude(c => c.UserAccount)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Where(a => a.VetProfileId == profile.Id && a.Status == "Pending")
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            var vm = new VetDashboardVM
            {
                Profile = profile,
                PendingAppointments = pendingAppointments,
                RecentReviews = profile.Reviews?.OrderByDescending(r => r.CreatedAt).Take(5).ToList() ?? new List<VetReview>()
            };

            return View(vm);
        }

        // --------------------------------------------------------
        // PUBLIC PROFILE VIEW
        // --------------------------------------------------------
        [AllowAnonymous]
        public async Task<IActionResult> Profile(int? id)
        {
            VetProfile vet = null;

            if (id.HasValue && id.Value > 0)
            {
                vet = await _context.VetProfiles
                    .Include(v => v.UserAccount)
                    .Include(v => v.Schedule)
                    .Include(v => v.Reviews).ThenInclude(r => r.Reviewer)
                    .FirstOrDefaultAsync(v => v.Id == id);
            }
            else if (User.Identity.IsAuthenticated && User.IsInRole("Vet"))
            {
                var user = await _userManager.GetUserAsync(User);
                vet = await _context.VetProfiles
                    .Include(v => v.UserAccount)
                    .Include(v => v.Schedule)
                    .Include(v => v.Reviews).ThenInclude(r => r.Reviewer)
                    .FirstOrDefaultAsync(v => v.UserAccountId == user.Id);

                if (vet == null) return RedirectToAction(nameof(Create));
            }

            if (vet == null) return NotFound();
            return View(vet);
        }

        // --------------------------------------------------------
        // CREATE PROFILE
        // --------------------------------------------------------
        [Authorize(Roles = "Vet")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            // Send them to edit if they already exist
            if (await _context.VetProfiles.AnyAsync(p => p.UserAccountId == user.Id))
                return RedirectToAction(nameof(Edit));

            var vm = new VetProfileFormVM { Email = user.Email, PhoneNumber = user.PhoneNumber };

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                vm.Schedule.Add(new WorkingDayVM { Day = day, IsOff = true });
            }

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Vet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VetProfileFormVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);

            // --- SAFETY NET: Prevent Double-Click Crashes ---
            var existingProfile = await _context.VetProfiles.FirstOrDefaultAsync(p => p.UserAccountId == user.Id);
            if (existingProfile != null)
            {
                return RedirectToAction("Profile", new { id = existingProfile.Id });
            }

            // Check for duplicate emails gracefully
            if (user.Email.ToLower() != vm.Email.ToLower() && await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError("Email", "This email is already in use by another account.");
                return View(vm);
            }

            // Update User Identity
            user.Email = vm.Email;
            user.UserName = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            user.IsProfileComplete = true; // Unlocks Dashboard
            await _userManager.UpdateAsync(user);

            // Create Profile
            var profile = new VetProfile
            {
                UserAccountId = user.Id,
                Specialization = vm.Specialization,
                ClinicName = vm.ClinicName,
                ClinicAddress = vm.ClinicAddress,
                YearsOfExperience = vm.YearsOfExperience,
                Description = vm.Description,
                Services = vm.Services, // Saved directly from JS tags
                ImageUrl = await ProcessUploadedFile(vm.ImageFile) ?? "/images/default-vet.png"
            };

            // Build Schedule
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

            // Redirect to their shiny new profile page
            return RedirectToAction("Profile", new { id = profile.Id });
        }

        // --------------------------------------------------------
        // EDIT PROFILE
        // --------------------------------------------------------
        [Authorize(Roles = "Vet")]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            var profile = await _context.VetProfiles
                .Include(p => p.Schedule)
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return RedirectToAction(nameof(Create));

            // --- AUTO-REPAIR: Add missing days directly to the profile object ---
            if (profile.Schedule == null || !profile.Schedule.Any())
            {
                profile.Schedule = new List<WorkingDay>();
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newDay = new WorkingDay { Day = day, IsOff = true, VetProfileId = profile.Id };
                    _context.WorkingDays.Add(newDay);
                    profile.Schedule.Add(newDay); // Crucial for immediate rendering
                }
                await _context.SaveChangesAsync();
            }

            var vm = new VetProfileFormVM
            {
                Id = profile.Id,
                ClinicName = profile.ClinicName,
                ClinicAddress = profile.ClinicAddress,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Specialization = profile.Specialization,
                YearsOfExperience = profile.YearsOfExperience,
                Description = profile.Description,
                Services = profile.Services ?? "",
                ExistingImageUrl = profile.ImageUrl,
                // Order schedule starting from Monday
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
        [Authorize(Roles = "Vet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VetProfileFormVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);

            // Check for duplicate emails safely
            if (user.Email.ToLower() != vm.Email.ToLower() && await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError("Email", "This email address is already in use.");
                return View(vm);
            }

            var profile = await _context.VetProfiles
                .Include(p => p.Schedule)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return NotFound();

            // 1. Update Identity
            user.Email = vm.Email;
            user.UserName = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            await _userManager.UpdateAsync(user);

            // 2. Update Profile
            profile.ClinicName = vm.ClinicName;
            profile.ClinicAddress = vm.ClinicAddress;
            profile.Specialization = vm.Specialization;
            profile.YearsOfExperience = vm.YearsOfExperience;
            profile.Description = vm.Description;
            profile.Services = vm.Services;

            if (vm.ImageFile != null)
            {
                profile.ImageUrl = await ProcessUploadedFile(vm.ImageFile);
            }

            // 3. Update Schedule (Safely parsing TimeSpans)
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

            // THE FIX: We must .Include() the Schedule so the database knows to grab the working days too!
            var profile = await _context.VetProfiles
                .Include(p => p.Schedule)
                .Include(p => p.Reviews) // Also including reviews just in case they block deletion!
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile != null)
            {
                // 1. Delete the child records FIRST (Working Days) to satisfy the database rules
                if (profile.Schedule != null && profile.Schedule.Any())
                {
                    _context.RemoveRange(profile.Schedule);
                }

                // 2. If they have reviews, delete those too
                if (profile.Reviews != null && profile.Reviews.Any())
                {
                    _context.RemoveRange(profile.Reviews);
                }

                // 3. Now that the children are gone, we can safely delete the parent profile
                _context.VetProfiles.Remove(profile);

                // 4. Relock the dashboard so they are forced back to Create next time
                user.IsProfileComplete = false;
                await _userManager.UpdateAsync(user);

                // 5. Save all the deletions!
                await _context.SaveChangesAsync();
            }

            // Send them straight back to the Create page
            return RedirectToAction("Create", "Vet");
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
            using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/vets/" + uniqueFileName;
        }
    }
}