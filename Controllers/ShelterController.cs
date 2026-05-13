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
    // This controller handles all shelter-related pages:
    // Dashboard, public profile, create, edit, and delete.
    public class ShelterController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ShelterController(
            AppDbContext context,
            UserManager<UserAccount> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // Shelter dashboard for logged-in shelter users only
        [Authorize(Roles = "Shelter")]
        public async Task<IActionResult> Dashboard()
        {
            // Get the logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Load the shelter profile and schedule
            var profile = await _context.ShelterProfiles
                .Include(p => p.UserAccount)
                .Include(p => p.Schedule)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            // If no profile exists yet, send them to create one
            if (profile == null) return RedirectToAction(nameof(Create));

            // Attach profile to the user object so layouts can use it if needed
            user.ShelterProfile = profile;

            // Build the dashboard view model
            var vm = new ShelterDashboardVM
            {
                Profile = profile,

                // Count the shelter services from the comma-separated string
                ServicesCount = string.IsNullOrWhiteSpace(profile.Services)
                    ? 0
                    : profile.Services.Split(',', StringSplitOptions.RemoveEmptyEntries).Length,

                // Count open days only
                OpenDaysCount = profile.Schedule?.Count(d => !d.IsOff) ?? 0,

                // Show only first few working days on dashboard
                SchedulePreview = profile.Schedule == null
                 ? Array.Empty<WorkingDay>()
                 : profile.Schedule
                  .OrderBy(d => ((int)d.Day + 6) % 7)
                   .Take(3)
                   .ToList()

            };

            return View(vm);
        }

        // Public profile page
        // If id is provided, show that shelter profile
        // If no id and current user is a shelter, show their own profile
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
            else if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Shelter"))
            {
                var user = await _userManager.GetUserAsync(User);

                shelter = await _context.ShelterProfiles
                    .Include(s => s.UserAccount)
                    .Include(s => s.Schedule)
                    .FirstOrDefaultAsync(s => s.UserAccountId == user!.Id);

                if (shelter == null) return RedirectToAction(nameof(Create));
            }

            if (shelter == null) return NotFound();

            return View(shelter);
        }

        // Show create form
        [Authorize(Roles = "Shelter")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // If profile already exists, go to edit page
            if (await _context.ShelterProfiles.AnyAsync(p => p.UserAccountId == user.Id))
                return RedirectToAction(nameof(Edit));

            // Pre-fill account contact data
            var vm = new ShelterProfileFormVM
            {
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            // Create default weekly schedule
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                vm.Schedule.Add(new WorkingDayVM
                {
                    Day = day,
                    IsOff = true
                });
            }

            return View(vm);
        }

        public async Task<IActionResult> Index()
        {
            // The .Include() tells the database to grab the linked UserAccount data too!
            var shelters = await _context.ShelterProfiles
                                         .Include(s => s.UserAccount)
                                         .ToListAsync();

            return View(shelters);
        }

        // Save new shelter profile
        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShelterProfileFormVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Prevent duplicate profile creation
            var existingProfile = await _context.ShelterProfiles
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (existingProfile != null)
            {
                return RedirectToAction(nameof(Profile), new { id = existingProfile.Id });
            }

            // Check email uniqueness
            if (user.Email!.ToLower() != vm.Email.ToLower() &&
                await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError("Email", "This email is already in use by another account.");
                return View(vm);
            }

            // Update user identity info
            user.Email = vm.Email;
            user.UserName = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            user.IsProfileComplete = true;
            await _userManager.UpdateAsync(user);

            // Create shelter profile entity
            var profile = new ShelterProfile
            {
                UserAccountId = user.Id,
                ShelterName = vm.ShelterName,
                ShelterAddress = vm.ShelterAddress,
                Description = vm.Description,
                Services = vm.Services,
                ImageUrl = await ProcessUploadedFile(vm.ImageFile) ?? "/images/default-shelter.png"
            };

            // Save schedule rows
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

            return RedirectToAction(nameof(Profile), new { id = profile.Id });
        }

        // Show edit form
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

            // If schedule is missing, create default rows
            if (profile.Schedule == null || !profile.Schedule.Any())
            {
                profile.Schedule = new System.Collections.Generic.List<WorkingDay>();

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newDay = new WorkingDay
                    {
                        Day = day,
                        IsOff = true,
                        ShelterProfileId = profile.Id
                    };

                    _context.WorkingDays.Add(newDay);
                    profile.Schedule.Add(newDay);
                }

                await _context.SaveChangesAsync();
            }

            // Fill the edit form view model
            var vm = new ShelterProfileFormVM
            {
                Id = profile.Id,
                ShelterName = profile.ShelterName,
                ShelterAddress = profile.ShelterAddress,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Description = profile.Description,
                Services = profile.Services ?? string.Empty,
                ExistingImageUrl = profile.ImageUrl,
                Schedule = profile.Schedule
                    .Select(w => new WorkingDayVM
                    {
                        Id = w.Id,
                        Day = w.Day,
                        IsOff = w.IsOff,
                        OpenTime = w.OpenTime?.ToString(@"hh\:mm"),
                        CloseTime = w.CloseTime?.ToString(@"hh\:mm")
                    })
                    .OrderBy(s => ((int)s.Day + 6) % 7)
                    .ToList()
            };

            return View(vm);
        }

        // Save edited shelter profile
        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShelterProfileFormVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Check email uniqueness
            if (user.Email!.ToLower() != vm.Email.ToLower() &&
                await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError("Email", "This email address is already in use.");
                return View(vm);
            }

            var profile = await _context.ShelterProfiles
                .Include(p => p.Schedule)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return NotFound();

            // Update identity info
            user.Email = vm.Email;
            user.UserName = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            await _userManager.UpdateAsync(user);

            // Update shelter profile fields
            profile.ShelterName = vm.ShelterName;
            profile.ShelterAddress = vm.ShelterAddress;
            profile.Description = vm.Description;
            profile.Services = vm.Services;

            // Replace image only if user uploaded a new one
            if (vm.ImageFile != null)
            {
                profile.ImageUrl = await ProcessUploadedFile(vm.ImageFile);
            }

            // Update schedule rows
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

            return RedirectToAction(nameof(Profile), new { id = profile.Id });
        }

        // Delete shelter profile
        [HttpPost]
        [Authorize(Roles = "Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.ShelterProfiles
                .Include(p => p.Schedule)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile != null)
            {
                // Delete child schedule rows first
                if (profile.Schedule != null && profile.Schedule.Any())
                {
                    _context.RemoveRange(profile.Schedule);
                }

                // Delete profile and relock account
                _context.ShelterProfiles.Remove(profile);
                user.IsProfileComplete = false;
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Create));
        }

        // Helper method to save uploaded shelter image
        private async Task<string?> ProcessUploadedFile(Microsoft.AspNetCore.Http.IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/shelters");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid() + "_" + file.FileName;

            using var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create);
            await file.CopyToAsync(fileStream);

            return "/uploads/shelters/" + uniqueFileName;
        }
    }
}
