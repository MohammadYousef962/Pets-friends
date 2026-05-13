using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
    [Authorize(Roles = "Merchant")]
    public class MerchantController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MerchantController(AppDbContext context, UserManager<UserAccount> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // --------------------------------------------------------
        // FILE UPLOAD HELPER
        // --------------------------------------------------------
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "merchants");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/uploads/merchants/" + uniqueFileName;
        }

        // --------------------------------------------------------
        // DASHBOARD
        // --------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.MerchantProfiles
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null)
            {
                return RedirectToAction("CreateProfile");
            }

            user.MerchantProfile = profile;

            // Fetch Products belonging to this specific merchant
            var storeProducts = await _context.Products
                .Where(p => p.MerchantProfileId == profile.Id)
                .ToListAsync();

            // Find items with stock of 5 or less
            var lowStock = storeProducts.Where(p => p.StockQuantity <= 5).ToList();

            // Fetch Orders
            var storeOrders = await _context.Orders
                .Include(o => o.ClientProfile)
                    .ThenInclude(c => c.UserAccount)
                // .Where(o => o.MerchantProfileId == profile.Id) 
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var activeOrders = storeOrders.Where(o => o.Status != "Delivered").ToList();

            var vm = new MerchantDashboardVM
            {
                Profile = profile,
                StoreProducts = storeProducts,
                LowStockProducts = lowStock,
                TotalProductsCount = storeProducts.Count,
                TotalRevenue = storeOrders.Sum(o => o.TotalAmount),
                ActiveOrdersCount = activeOrders.Count,
                RecentOrders = storeOrders.Take(5).ToList()
            };

            return View(vm);
        }

        // --------------------------------------------------------
        // CREATE PROFILE
        // --------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> CreateProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (await _context.MerchantProfiles.AnyAsync(p => p.UserAccountId == user.Id))
            {
                return RedirectToAction("Dashboard");
            }

            var vm = new MerchantProfileFormVM
            {
                FullName = user.FullName,
                ContactEmail = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(MerchantProfileFormVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (await _context.MerchantProfiles.AnyAsync(p => p.UserAccountId == user.Id))
            {
                return RedirectToAction("Dashboard");
            }

            string? uploadedImagePath = null;
            if (model.ImageFile != null)
            {
                uploadedImagePath = await SaveImageAsync(model.ImageFile);
            }

            // 1. Update main UserAccount details (NOW INCLUDES EMAIL)
            user.FullName = model.FullName;
            user.Email = model.ContactEmail;
            user.UserName = model.ContactEmail; // Keep login username synced
            user.PhoneNumber = model.PhoneNumber;
            user.IsProfileComplete = true;
            await _userManager.UpdateAsync(user);

            // 2. Create Profile
            var profile = new MerchantProfile
            {
                UserAccountId = user.Id,
                StoreName = model.StoreName,
                StoreAddress = model.StoreAddress,
                ImageUrl = uploadedImagePath
            };

            _context.MerchantProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        // --------------------------------------------------------
        // EDIT PROFILE
        // --------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.MerchantProfiles.FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return RedirectToAction("CreateProfile");

            var vm = new MerchantProfileFormVM
            {
                Id = profile.Id,
                FullName = user.FullName,
                StoreName = profile.StoreName,
                StoreAddress = profile.StoreAddress,
                ContactEmail = user.Email,
                PhoneNumber = user.PhoneNumber,
                ExistingImageUrl = profile.ImageUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(MerchantProfileFormVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.MerchantProfiles.FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return RedirectToAction("CreateProfile");

            if (model.ImageFile != null)
            {
                profile.ImageUrl = await SaveImageAsync(model.ImageFile);
            }

            // Update User (NOW INCLUDES EMAIL)
            user.FullName = model.FullName;
            user.Email = model.ContactEmail;
            user.UserName = model.ContactEmail; // Keep login username synced
            user.PhoneNumber = model.PhoneNumber;
            await _userManager.UpdateAsync(user);

            // Update Store
            profile.StoreName = model.StoreName;
            profile.StoreAddress = model.StoreAddress;

            _context.MerchantProfiles.Update(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
    }
}