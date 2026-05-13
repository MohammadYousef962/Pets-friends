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
    [Authorize] // Ensures users must be logged in to access anything here
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        public OrderController(AppDbContext context, UserManager<UserAccount> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ====================================================================
        // 1. CLIENT ENDPOINTS (Buying & History)
        // ====================================================================

        [Authorize(Roles = "Client,Vet,Shelter")]
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(c => c.UserAccountId == user.Id);

            if (clientProfile == null) return RedirectToAction("Index", "Home");

            // Pull all orders placed strictly by this client
            var orders = await _context.Orders
                .Include(o => o.MerchantProfile) // To show the store name
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.ClientProfileId == clientProfile.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders); // Renders a client-facing history view
        }

        // ====================================================================
        // 2. MERCHANT ENDPOINTS (Fulfillment Dashboard)
        // ====================================================================

        [Authorize(Roles = "Merchant")]
        [HttpGet]
        public async Task<IActionResult> StoreOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var merchantProfile = await _context.MerchantProfiles
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (merchantProfile == null) return RedirectToAction("CreateProfile", "Merchant");

            // Pull strictly orders assigned to this merchant's store
            var orders = await _context.Orders
                .Include(o => o.ClientProfile)
                    .ThenInclude(c => c.UserAccount) // To show buyer's name
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.MerchantProfileId == merchantProfile.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.MerchantProfiles
                .FirstOrDefaultAsync(p => p.UserAccountId == user.Id);

            if (profile == null) return RedirectToAction("CreateProfile", "Merchant");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.MerchantProfileId == profile.Id);

            if (order != null)
            {
                var validStatuses = new[] { "Pending", "Shipped", "Delivered", "Cancelled" };
                if (validStatuses.Contains(newStatus))
                {
                    order.Status = newStatus;
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("StoreOrders");
        }

        // ====================================================================
        // 3. SHARED ENDPOINT: DIGITAL RECEIPT / INVOICE
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Eager-loading product collections guarantees the UI loops execute safely
            var order = await _context.Orders
                .Include(o => o.ClientProfile)
                    .ThenInclude(c => c.UserAccount)
                .Include(o => o.MerchantProfile)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            bool isBuyer = order.ClientProfile != null && order.ClientProfile.UserAccountId == user.Id;
            bool isSeller = order.MerchantProfile != null && order.MerchantProfile.UserAccountId == user.Id;

            if (!isBuyer && !isSeller)
            {
                return Forbid();
            }

            return View(order);
        }

        // ====================================================================
        // 4. NEW INTEGRATION: SECURE CHECKOUT INTERFACE
        // ====================================================================
        [HttpGet]
        [Authorize(Roles = "Client,Vet,Shelter")]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(c => c.UserAccountId == user.Id);

            if (clientProfile == null) return RedirectToAction("Home", "Store");

            var cartItems = await _context.ShoppingCarts
                .Include(c => c.Product)
                    .ThenInclude(p => p.MerchantProfile)
                .Where(c => c.UserAccountId == user.Id)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Cart", "Store");

            // Evaluates pure decimal logic to guarantee structural database parity
            decimal subtotal = cartItems.Sum(i => i.Quantity * (decimal)i.Product.Price);
            decimal tax = subtotal * 0.08m;

            var viewModel = new CheckoutVM
            {
                FullName = user.FullName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                StreetAddress = string.Empty,
                City = string.Empty,

                CartItems = cartItems,
                Subtotal = subtotal,
                Tax = tax,
                GrandTotal = subtotal + tax
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Client,Vet,Shelter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(c => c.UserAccountId == user.Id);

            if (clientProfile == null) return RedirectToAction("Home", "Store");

            var cartItems = await _context.ShoppingCarts
                .Include(c => c.Product)
                .Where(c => c.UserAccountId == user.Id)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Cart", "Store");

            // 1. Permanently update profile phone record if supplied freshly
            if (ModelState.IsValid && string.IsNullOrEmpty(user.PhoneNumber))
            {
                user.PhoneNumber = model.PhoneNumber;
                await _userManager.UpdateAsync(user);
            }

            // 2. Isolate independent vendor store drop shipments cleanly
            var merchantGroups = cartItems.GroupBy(c => c.Product.MerchantProfileId);

            foreach (var group in merchantGroups)
            {
                // Evaluates pure decimal logic to guarantee structural database parity
                decimal groupSubtotal = group.Sum(i => i.Quantity * (decimal)i.Product.Price);
                decimal groupTax = groupSubtotal * 0.08m;
                decimal combinedTotal = groupSubtotal + groupTax;

                var order = new Order
                {
                    ClientProfileId = clientProfile.Id,
                    MerchantProfileId = group.Key,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    // FIXED: Assigned pure decimal directly to match EF Core Model definitions perfectly
                    TotalAmount = combinedTotal,
                    OrderItems = new List<OrderItem>()
                };

                foreach (var item in group)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price
                    });

                    // Insulate item stock safely avoiding negative integer overflow
                    if (item.Product.StockQuantity >= item.Quantity)
                        item.Product.StockQuantity -= item.Quantity;
                    else
                        item.Product.StockQuantity = 0;

                    _context.Products.Update(item.Product);
                }

                _context.Orders.Add(order);
            }

            // 3. Purge bag cleanly to signal fulfillment loop execution
            _context.ShoppingCarts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["OrderSuccess"] = true;
            return RedirectToAction("MyOrders");
        }
    }
}