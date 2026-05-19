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
    // 1. BASE RULE: Only these 3 roles can execute actions in this controller by default
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
        [AllowAnonymous] // Overrides the base rule: Guests, Admins, and Merchants can LOOK, but they can't touch.
        public async Task<IActionResult> Home()
        {
            // Cleanly route Admins & Merchants away from the buyer storefront if they wander in
            if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("Merchant")))
            {
                return RedirectToAction("AccessDenied", "Account");
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
            if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("Merchant")))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var product = await _context.Products
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.ClientProfile)
                        .ThenInclude(c => c.UserAccount)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var viewModel = new ProductDetailsVM
            {
                Product = product,
                Reviews = product.Reviews?.OrderByDescending(r => r.ReviewDate).ToList() ?? new List<ProductReview>(),
                AverageRating = product.Reviews != null && product.Reviews.Any() ? Math.Round(product.Reviews.Average(r => r.Rating), 1) : 0,
                TotalReviews = product.Reviews?.Count ?? 0,
                SelectedQuantity = 1
            };

            return View(viewModel);
        }

        // ====================================================================
        // 3. POST: PROCESS "ADD TO CART" BUY BOX SUBMISSION
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        // No [AllowAnonymous] here! If a guest clicks Add to Cart, ASP.NET natively bounces them to Login.
        // If an Admin/Merchant bypasses the UI and posts here, ASP.NET natively bounces them to AccessDenied.
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
                _context.ShoppingCarts.Add(new ShoppingCart
                {
                    ProductId = productId,
                    UserAccountId = user.Id,
                    Quantity = quantity
                });
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

            return View(new CartVM { Items = cartItems });
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
            if (User.Identity?.IsAuthenticated != true || User.IsInRole("Admin") || User.IsInRole("Merchant"))
                return Json(0);

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
                _context.ProductReviews.Add(new ProductReview
                {
                    ProductId = ProductId,
                    ClientProfileId = clientProfile.Id,
                    Rating = Rating,
                    Comment = Comment,
                    ReviewDate = DateTime.Now
                });
                TempData["ReviewConfirmation"] = "Published";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = ProductId });
        }
        // ====================================================================
        // 10. POST: DELETE PRODUCT REVIEW
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int ProductId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            if (clientProfile != null)
            {
                var existingReview = await _context.ProductReviews
                    .FirstOrDefaultAsync(r => r.ProductId == ProductId && r.ClientProfileId == clientProfile.Id);

                if (existingReview != null)
                {
                    _context.ProductReviews.Remove(existingReview);
                    await _context.SaveChangesAsync();

                    // This links to the popup modal script we added to Details.cshtml
                    TempData["ReviewConfirmation"] = "Deleted";
                }
            }
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
                _context.ShoppingCarts.Add(new ShoppingCart
                {
                    ProductId = productId,
                    UserAccountId = user.Id,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();

            int distinctCount = await _context.ShoppingCarts
                .Where(c => c.UserAccountId == user.Id)
                .CountAsync();

            return Json(new { success = true, cartCount = distinctCount });
        }
    }
}