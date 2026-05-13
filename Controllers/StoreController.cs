using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Data.ViewModels;
using Pets_friends.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pets_friends.Controllers
{
    [Authorize(Roles = "Client,Vet,Shelter")]
    public class StoreController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        public StoreController(AppDbContext context, UserManager<UserAccount> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ====================================================================
        // 1. GET: MAIN MULTI-VENDOR CATALOG (Public Entrance)
        // ====================================================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Home()
        {
            // Intercept Admin & Merchant: Pass their previous URL to AccessDenied
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin") || User.IsInRole("Merchant"))
                {
                    string previousUrl = Request.Headers["Referer"].ToString();
                    return RedirectToAction("AccessDenied", "Account", new { returnUrl = previousUrl });
                }
            }

            var products = await _context.Products
                .Include(p => p.MerchantProfile)
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        // ====================================================================
        // 2. GET: PRODUCT DETAILS & REVIEWS PAGE (PDP)
        // ====================================================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            // Intercept Admin & Merchant: Pass their previous URL to AccessDenied
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin") || User.IsInRole("Merchant"))
                {
                    string previousUrl = Request.Headers["Referer"].ToString();
                    return RedirectToAction("AccessDenied", "Account", new { returnUrl = previousUrl });
                }
            }

            var product = await _context.Products
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.ClientProfile)
                        .ThenInclude(c => c.UserAccount)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            double avgScore = 0;
            int reviewCount = product.Reviews?.Count ?? 0;

            if (reviewCount > 0)
            {
                avgScore = product.Reviews!.Average(r => r.Rating);
            }

            var viewModel = new ProductDetailsVM
            {
                Product = product,
                Reviews = product.Reviews?.OrderByDescending(r => r.ReviewDate).ToList() ?? new List<ProductReview>(),
                AverageRating = Math.Round(avgScore, 1),
                TotalReviews = reviewCount,
                SelectedQuantity = 1
            };

            return View(viewModel);
        }

        // ====================================================================
        // 3. POST: PROCESS "ADD TO CART" BUY BOX SUBMISSION
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingItem = await _context.ShoppingCarts
                .FirstOrDefaultAsync(n => n.ProductId == productId && n.UserAccountId == user.Id);

            if (existingItem != null)
            {
                int actualStock = (await _context.Products.FindAsync(productId))?.StockQuantity ?? 0;
                int maxAllowed = Math.Min(10, actualStock);

                existingItem.Quantity = Math.Min(maxAllowed, existingItem.Quantity + quantity);
            }
            else
            {
                var cartItem = new ShoppingCart()
                {
                    ProductId = productId,
                    UserAccountId = user.Id,
                    Quantity = quantity
                };
                _context.ShoppingCarts.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            TempData["ShowCartModal"] = true;
            TempData["LastAddedItem"] = (await _context.Products.FindAsync(productId))?.Name;

            return RedirectToAction("Details", new { id = productId });
        }

        // ====================================================================
        // 4. GET: THE SHOPPING BAG PAGE
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cartItems = await _context.ShoppingCarts
                .Include(c => c.Product)
                    .ThenInclude(p => p.MerchantProfile)
                .Where(c => c.UserAccountId == user.Id)
                .Select(c => new Pets_friends.Data.ViewModels.CartItem
                {
                    Id = c.Id,
                    Product = c.Product,
                    Quantity = c.Quantity
                }).ToListAsync();

            var viewModel = new CartVM { Items = cartItems };
            return View(viewModel);
        }

        // ====================================================================
        // 5. POST: UPDATE QUANTITY IN CART
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int newQuantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cartEntry = await _context.ShoppingCarts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserAccountId == user.Id);

            if (cartEntry != null && newQuantity > 0)
            {
                int maxAllowed = Math.Min(10, cartEntry.Product.StockQuantity);

                if (newQuantity <= maxAllowed)
                {
                    cartEntry.Quantity = newQuantity;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Cart");
        }

        // ====================================================================
        // 6. POST: REMOVE FROM CART
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cartEntry = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.Id == id && c.UserAccountId == user.Id);

            if (cartEntry != null)
            {
                _context.ShoppingCarts.Remove(cartEntry);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Cart");
        }

        // ====================================================================
        // 7. GET: LIVE CART COUNTER ENDPOINT
        // ====================================================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCartCount()
        {
            if (User.Identity?.IsAuthenticated != true) return Json(0);

            if (User.IsInRole("Admin") || User.IsInRole("Merchant")) return Json(0);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(0);

            int count = await _context.ShoppingCarts
                .Where(c => c.UserAccountId == user.Id)
                .CountAsync();

            return Json(count);
        }

        // ====================================================================
        // 8. POST: SUBMIT PRODUCT REVIEW
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int ProductId, int Rating, string Comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            if (clientProfile == null)
            {
                clientProfile = new ClientProfile { UserAccountId = user.Id };
                _context.ClientProfiles.Add(clientProfile);
                await _context.SaveChangesAsync();
            }

            var existingReview = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.ProductId == ProductId && r.ClientProfileId == clientProfile.Id);

            if (existingReview != null)
            {
                existingReview.Rating = Rating;
                existingReview.Comment = Comment;
                existingReview.ReviewDate = DateTime.Now;
                TempData["ReviewConfirmation"] = "Updated";
            }
            else
            {
                var newReview = new ProductReview
                {
                    ProductId = ProductId,
                    ClientProfileId = clientProfile.Id,
                    Rating = Rating,
                    Comment = Comment,
                    ReviewDate = DateTime.Now
                };
                _context.ProductReviews.Add(newReview);
                TempData["ReviewConfirmation"] = "Published";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = ProductId });
        }

        // ====================================================================
        // 9. POST: BACKGROUND AJAX "ADD TO CART"
        // ====================================================================
        [HttpPost]
        public async Task<IActionResult> AddToCartAjax(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false });

            var existingItem = await _context.ShoppingCarts
                .FirstOrDefaultAsync(n => n.ProductId == productId && n.UserAccountId == user.Id);

            if (existingItem != null)
            {
                int actualStock = (await _context.Products.FindAsync(productId))?.StockQuantity ?? 0;
                int maxAllowed = Math.Min(10, actualStock);

                existingItem.Quantity = Math.Min(maxAllowed, existingItem.Quantity + quantity);
            }
            else
            {
                var cartItem = new ShoppingCart()
                {
                    ProductId = productId,
                    UserAccountId = user.Id,
                    Quantity = quantity
                };
                _context.ShoppingCarts.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            int distinctCount = await _context.ShoppingCarts
                .Where(c => c.UserAccountId == user.Id)
                .CountAsync();

            return Json(new { success = true, cartCount = distinctCount });
        }
    }
}