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
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(AppDbContext context, UserManager<UserAccount> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // Helper to grab the logged-in merchant's profile
        private async Task<MerchantProfile?> GetCurrentMerchantAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _context.MerchantProfiles.FirstOrDefaultAsync(p => p.UserAccountId == user.Id);
        }

        // Helper to save product images
        private async Task<string> SaveProductImageAsync(IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/uploads/products/" + uniqueFileName;
        }

        // --------------------------------------------------------
        // 1. INVENTORY LIST (CARD GRID)
        // --------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Home()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.MerchantProfiles.FirstOrDefaultAsync(p => p.UserAccountId == user.Id);
            if (profile == null) return RedirectToAction("CreateProfile", "Merchant");

            ViewData["MerchantAvatar"] = profile.ImageUrl;
            ViewData["MerchantName"] = user.FullName ?? profile.StoreName;

            var products = await _context.Products
                .Where(p => p.MerchantProfileId == profile.Id)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        // --------------------------------------------------------
        // 2. CREATE PRODUCT
        // --------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.MerchantProfiles.FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            ViewData["MerchantAvatar"] = profile?.ImageUrl;
            ViewData["MerchantName"] = user?.FullName ?? profile?.StoreName;

            return View(new ProductFormVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormVM model)
        {
            var profile = await GetCurrentMerchantAsync();
            if (profile == null) return RedirectToAction("CreateProfile", "Merchant");

            // FIX 1: Ignore validation on the file specifically so it doesn't block the save
            ModelState.Remove("ImageFile");
            ModelState.Remove("ExistingImageUrl");

            if (!ModelState.IsValid)
            {
                // FIX 2: If another field (like description) fails, we MUST reload the navbar variables
                var user = await _userManager.GetUserAsync(User);
                ViewData["MerchantAvatar"] = profile.ImageUrl;
                ViewData["MerchantName"] = user?.FullName ?? profile.StoreName;
                return View(model);
            }

            string uploadedImagePath = "https://placehold.co/300x300/FAF6F1/8C7560?text=No+Image";
            if (model.ImageFile != null)
            {
                uploadedImagePath = await SaveProductImageAsync(model.ImageFile);
            }

            var product = new Product
            {
                MerchantProfileId = profile.Id,
                Name = model.Name,
                Category = model.Category,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                Description = model.Description,
                ImageUrl = uploadedImagePath
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Home");
        }

        // --------------------------------------------------------
        // 3. EDIT PRODUCT
        // --------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.MerchantProfiles.FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            ViewData["MerchantAvatar"] = profile?.ImageUrl;
            ViewData["MerchantName"] = user?.FullName ?? profile?.StoreName;

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.MerchantProfileId == profile.Id);
            if (product == null) return NotFound();

            var vm = new ProductFormVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category,
                ExistingImageUrl = product.ImageUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductFormVM model)
        {
            var profile = await GetCurrentMerchantAsync();
            if (profile == null) return RedirectToAction("CreateProfile", "Merchant");

            // FIX 1: Ignore validation on the file so you can save changes without re-uploading an image!
            ModelState.Remove("ImageFile");
            ModelState.Remove("ExistingImageUrl");

            if (!ModelState.IsValid)
            {
                // FIX 2: Reload layout variables so it doesn't crash visually
                var user = await _userManager.GetUserAsync(User);
                ViewData["MerchantAvatar"] = profile.ImageUrl;
                ViewData["MerchantName"] = user?.FullName ?? profile.StoreName;
                return View(model);
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.MerchantProfileId == profile.Id);

            if (product == null) return NotFound();

            if (model.ImageFile != null)
            {
                product.ImageUrl = await SaveProductImageAsync(model.ImageFile);
            }

            product.Name = model.Name;
            product.Category = model.Category;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.Description = model.Description;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Home");
        }

        // --------------------------------------------------------
        // 4. DELETE PRODUCT
        // --------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var profile = await GetCurrentMerchantAsync();
            if (profile == null) return RedirectToAction("CreateProfile", "Merchant");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.MerchantProfileId == profile.Id);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Home");
        }
    }
}