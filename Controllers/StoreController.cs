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
    [Authorize]
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
        // 1. GET: MAIN MULTI-VENDOR CATALOG (The Mall Entrance)
        // ====================================================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Home()
        {
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
        [Authorize(Roles = "Client,Vet,Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Check if item already exists in this specific user's cart
            var existingItem = await _context.ShoppingCarts
                .FirstOrDefaultAsync(n => n.ProductId == productId && n.UserAccountId == user.Id);

            if (existingItem != null)
            {
                // If it exists, stack the quantities
                existingItem.Quantity += quantity;
            }
            else
            {
                // If it's brand new, create the entry
                var cartItem = new ShoppingCart()
                {
                    ProductId = productId,
                    UserAccountId = user.Id,
                    Quantity = quantity
                };
                _context.ShoppingCarts.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // Trigger the professional post-purchase Decision Modal
            TempData["ShowCartModal"] = true;
            TempData["LastAddedItem"] = (await _context.Products.FindAsync(productId))?.Name;

            return RedirectToAction("Details", new { id = productId });
        }

        // ====================================================================
        // 4. GET: THE SHOPPING CART PAGE
        // ====================================================================
        [HttpGet]
        [Authorize(Roles = "Client,Vet,Shelter")]
        public async Task<IActionResult> Cart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Pull items cleanly, mapping strictly to the ViewModel namespace to prevent conflicts
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
        // 5. POST: REMOVE FROM CART (New Action!)
        // ====================================================================
        [HttpPost]
        [Authorize(Roles = "Client,Vet,Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Locate the exact cart entry securely verifying ownership
            var cartEntry = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.Id == id && c.UserAccountId == user.Id);

            if (cartEntry != null)
            {
                _context.ShoppingCarts.Remove(cartEntry);
                await _context.SaveChangesAsync();
            }

            // Instantly refresh the cart view
            return RedirectToAction("Cart");
        }
    }
}