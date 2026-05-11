using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Models;
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

            // Renders the beautiful wide-card view we just built!
            // (Make sure to move your Orders.cshtml to Views/Order/StoreOrders.cshtml)
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

        [HttpGet]
        // ====================================================================
        // SHARED ENDPOINT: DIGITAL RECEIPT / INVOICE
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Pull the order along with the Buyer name, Store name, and itemized Products
            var order = await _context.Orders
                .Include(o => o.ClientProfile)
                    .ThenInclude(c => c.UserAccount)
                .Include(o => o.MerchantProfile)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // STRICT DUAL-SECURITY CHECK: 
            // Allow access ONLY if the user is the Buyer (Client/Vet) who bought it OR the Merchant selling it.
            bool isBuyer = order.ClientProfile != null && order.ClientProfile.UserAccountId == user.Id;
            bool isSeller = order.MerchantProfile != null && order.MerchantProfile.UserAccountId == user.Id;

            if (!isBuyer && !isSeller)
            {
                return Forbid(); // Instantly blocks unauthorized users from snooping on receipts
            }

            return View(order);
        }
    }
}