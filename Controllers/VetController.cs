using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Data.ViewModels;
using Pets_friends.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

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

        // ====================================================================
        // 1. VET PRIVATE DASHBOARD
        // ====================================================================
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

        // ====================================================================
        // 2. PUBLIC PROFILE VIEW
        // ====================================================================
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

        // ====================================================================
        // 3. SECURE MANAGEMENT (Create, Edit, Delete)
        // ====================================================================

        [Authorize(Roles = "Vet")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (await _context.VetProfiles.AnyAsync(p => p.UserAccountId == user.Id))
                return RedirectToAction(nameof(Edit));

            var vm = new VetProfileFormVM();
            // Initialize empty schedule for a fresh start
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                vm.Schedule.Add(new WorkingDayVM { Day = day, IsOff = true });
            }
            return View("Edit", vm);
        }

        [HttpPost]
        [Authorize(Roles = "Vet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VetProfileFormVM vm)
        {
            if (!ModelState.IsValid) return View("Edit", vm);

            var user = await _userManager.GetUserAsync(User);
            var newProfile = new VetProfile
            {
                UserAccountId = user.Id,
                Specialization = vm.Specialization,
                ClinicName = vm.ClinicName,
                ClinicAddress = vm.ClinicAddress,
                YearsOfExperience = vm.YearsOfExperience,
                Description = vm.Description,
                Services = vm.Services,
                // Handle the physical file upload
                ImageUrl = await ProcessUploadedFile(vm.ImageFile) ?? "/images/default-vet.png"
            };

            foreach (var item in vm.Schedule)
            {
                newProfile.Schedule.Add(new WorkingDay
                {
                    Day = item.Day,
                    IsOff = item.IsOff,
                    OpenTime = (!item.IsOff && DateTime.TryParse(item.OpenTime, out var ot)) ? ot.TimeOfDay : null,
                    CloseTime = (!item.IsOff && DateTime.TryParse(item.CloseTime, out var ct)) ? ct.TimeOfDay : null
                });
            }

            _context.VetProfiles.Add(newProfile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Dashboard));
        }

        [Authorize(Roles = "Vet")]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            // CRITICAL: .Include(p => p.Schedule) ensures the hours are loaded!
            var profile = await _context.VetProfiles
                .Include(p => p.Schedule)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return RedirectToAction(nameof(Create));

            var vm = new VetProfileFormVM
            {
                Id = profile.Id,
                Specialization = profile.Specialization,
                ClinicName = profile.ClinicName,
                ClinicAddress = profile.ClinicAddress,
                YearsOfExperience = profile.YearsOfExperience,
                Description = profile.Description,
                ExistingImageUrl = profile.ImageUrl,
                Services = profile.Services,
                // Format times strictly for HTML5 time picker (HH:mm)
                Schedule = profile.Schedule.Select(w => new WorkingDayVM
                {
                    Id = w.Id,
                    Day = w.Day,
                    IsOff = w.IsOff,
                    OpenTime = w.OpenTime?.ToString(@"hh\:mm"),
                    CloseTime = w.CloseTime?.ToString(@"hh\:mm")
                }).OrderBy(s => ((int)s.Day + 6) % 7).ToList() // Sort Mon-Sun
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

            // CRITICAL: .Include(p => p.Schedule) so we can update existing hours
            var profile = await _context.VetProfiles
                .Include(p => p.Schedule)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return NotFound();

            profile.Specialization = vm.Specialization;
            profile.ClinicName = vm.ClinicName;
            profile.ClinicAddress = vm.ClinicAddress;
            profile.YearsOfExperience = vm.YearsOfExperience;
            profile.Description = vm.Description;
            profile.Services = vm.Services;

            // Physical File Upload Logic
            if (vm.ImageFile != null)
            {
                // Delete old image file to save server space
                if (!string.IsNullOrEmpty(profile.ImageUrl) && !profile.ImageUrl.Contains("placehold.co"))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, profile.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
                profile.ImageUrl = await ProcessUploadedFile(vm.ImageFile);
            }

            // Sync Schedule updates
            foreach (var item in vm.Schedule)
            {
                var dbDay = profile.Schedule.FirstOrDefault(w => w.Day == item.Day);
                if (dbDay != null)
                {
                    dbDay.IsOff = item.IsOff;
                    dbDay.OpenTime = (!item.IsOff && DateTime.TryParse(item.OpenTime, out var ot)) ? ot.TimeOfDay : null;
                    dbDay.CloseTime = (!item.IsOff && DateTime.TryParse(item.CloseTime, out var ct)) ? ct.TimeOfDay : null;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile", "Vet");
        }

        // Helper Method for Professional File Saving
        private async Task<string?> ProcessUploadedFile(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/vets");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/vets/" + uniqueFileName;
        }

        [HttpPost]
        [Authorize(Roles = "Vet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.VetProfiles.FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile != null)
            {
                _context.VetProfiles.Remove(profile);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile deleted permanently.";
            }

            return RedirectToAction("Dashboard", "Vet");
        }
    }
}