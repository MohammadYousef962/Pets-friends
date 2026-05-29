using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Data.ViewModels;
using Pets_friends.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pets_friends.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        public AdminController(AppDbContext context, UserManager<UserAccount> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================================
        // 1. DASHBOARD
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["AdminName"] = user?.FullName ?? "System Admin";

            // Pull delivered orders WITH their items to calculate exact tax
            var deliveredOrders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.Status == "Delivered")
                .ToListAsync();

            // 1. GROSS REVENUE: The absolute total amount paid by customers (Base Price + 8% Tax)
            decimal storeGrossRevenue = deliveredOrders.Sum(o => o.TotalAmount);

            // 2. PURE REVENUE: The base price of the items (Quantity * UnitPrice) before the 8% was added
            decimal pureRevenue = deliveredOrders.Sum(o => o.OrderItems.Sum(item => item.Quantity * (decimal)item.UnitPrice));

            // 3. EXACT TAX: The difference between what the customer paid and what the items cost
            decimal totalTax = storeGrossRevenue - pureRevenue;

            // Fallback exact math safeguard
            if (totalTax <= 0 && storeGrossRevenue > 0)
            {
                totalTax = (storeGrossRevenue / 1.08m) * 0.08m;
            }

            // Exact User Count (Matches ManageUsers page, ignores ghost accounts without roles)
            var actualUserCount = await (from u in _context.Users
                                         join ur in _context.UserRoles on u.Id equals ur.UserId
                                         select u.Id).Distinct().CountAsync();

            var vm = new AdminDashboardVM
            {
                TotalUsers = actualUserCount,

                // Only count orders that are truly active (Pending or Shipped)
                TotalOrders = await _context.Orders
                    .CountAsync(o => o.Status == "Pending" || o.Status == "Shipped"),

                // FIX: Admin now views the FULL GROSS amount paid by customers ($450.46)
                TotalRevenue = storeGrossRevenue,

                // Admin sees the exact 8% taken from the base price ($33.36)
                TotalTax = totalTax,

                // Only count appointments that are upcoming (Pending or Confirmed)
                TotalAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == "Pending" || a.Status == "Confirmed")
            };

            return View(vm);
        }

        // ==========================================================
        // 2. TRANSACTION HISTORY
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> Transactions()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["AdminName"] = user?.FullName ?? "System Admin";

            var orders = await _context.Orders
                .Include(o => o.ClientProfile).ThenInclude(c => c.UserAccount)
                .Select(o => new TransactionRecordVM
                {
                    ReferenceId = o.Id,
                    Timestamp = o.OrderDate,
                    Type = "Product",
                    CustomerName = (o.ClientProfile != null && o.ClientProfile.UserAccount != null) ? o.ClientProfile.UserAccount.FullName : "Unknown",
                    Details = "Store Purchase",
                    Amount = o.TotalAmount,
                    Status = o.Status
                }).ToListAsync();

            var appointments = await _context.Appointments
                .Include(a => a.ClientProfile).ThenInclude(c => c.UserAccount)
                .Include(a => a.Service)
                .Include(a => a.Pet)
                .Select(a => new TransactionRecordVM
                {
                    ReferenceId = a.Id,
                    Timestamp = a.AppointmentDate,
                    Type = "Service",
                    CustomerName = (a.ClientProfile != null && a.ClientProfile.UserAccount != null) ? a.ClientProfile.UserAccount.FullName : "Unknown",
                    Details = (a.Service != null ? a.Service.Name : "Service") + " for " + (a.Pet != null ? a.Pet.Name : "Pet"),
                    Amount = null,
                    Status = a.Status
                }).ToListAsync();

            var allTransactions = orders.Concat(appointments)
                .OrderByDescending(t => t.Timestamp)
                .ToList();

            return View(allTransactions);
        }

        // ==========================================================
        // 3. MANAGE ACCOUNTS 
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["AdminName"] = user?.FullName ?? "System Admin";

            var userVMs = await (from u in _context.Users
                                 join userRole in _context.UserRoles on u.Id equals userRole.UserId
                                 join role in _context.Roles on userRole.RoleId equals role.Id
                                 select new ManageUserVM
                                 {
                                     UserId = u.Id,
                                     FullName = u.FullName,
                                     Email = u.Email,
                                     CurrentRole = role.Name
                                 }).ToListAsync();

            return View(userVMs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains("Admin") && newRole != "Admin")
            {
                TempData["ErrorMessage"] = "Safety check: Cannot demote an Admin via this panel.";
                return RedirectToAction(nameof(ManageUsers));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);

                // Target and destroy ALL profile data that does not belong to the NEW role.
                if (newRole != "Client") await PurgeRoleDataAsync(user.Id, "Client");
                if (newRole != "Vet") await PurgeRoleDataAsync(user.Id, "Vet");
                if (newRole != "Shelter") await PurgeRoleDataAsync(user.Id, "Shelter");
                if (newRole != "Merchant") await PurgeRoleDataAsync(user.Id, "Merchant");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Role updated to {newRole} for {user.FullName}. Irrelevant data dropped.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"Role conversion failed. Alterations rolled back. Error: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        // ==========================================================
        // 4. DELETE ACCOUNT 
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "You cannot delete yourself.";
                return RedirectToAction(nameof(ManageUsers));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Unconditionally destroy all child relationships across ALL roles
                await PurgeRoleDataAsync(user.Id, "Client");
                await PurgeRoleDataAsync(user.Id, "Vet");
                await PurgeRoleDataAsync(user.Id, "Shelter");
                await PurgeRoleDataAsync(user.Id, "Merchant");

                // Clear base-level user data completely
                var carts = _context.ShoppingCarts.Where(c => c.UserAccountId == user.Id);
                _context.ShoppingCarts.RemoveRange(carts);

                var vetReviewsAsClient = _context.VetReviews.Where(r => r.ReviewerId == user.Id);
                _context.VetReviews.RemoveRange(vetReviewsAsClient);

                await _context.SaveChangesAsync();

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded) throw new Exception("Identity engine rejected deletion.");

                await transaction.CommitAsync();
                TempData["SuccessMessage"] = "User identity and all associated profiles completely erased.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"Purge routine failed. Error: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        // ==========================================================
        // 5. THE MASTER PURGE SCRIPT (Bulletproof Delete Behavior)
        // ==========================================================
        private async Task PurgeRoleDataAsync(string userId, string roleToPurge)
        {
            if (roleToPurge == "Client")
            {
                var client = await _context.ClientProfiles.Include(c => c.Pets).FirstOrDefaultAsync(c => c.UserAccountId == userId);
                if (client != null)
                {
                    // Bottom-Up dependent deletion to bypass SQL NO_ACTION constraints
                    var productReviews = _context.ProductReviews.Where(pr => pr.ClientProfileId == client.Id);
                    _context.ProductReviews.RemoveRange(productReviews);

                    var apps = _context.AdoptionApplications.Where(a => a.ClientProfileId == client.Id);
                    _context.AdoptionApplications.RemoveRange(apps);

                    var appointments = _context.Appointments.Where(a => a.ClientProfileId == client.Id);
                    _context.Appointments.RemoveRange(appointments);

                    var orders = _context.Orders.Where(o => o.ClientProfileId == client.Id);
                    var orderItems = _context.OrderItems.Where(oi => orders.Select(o => o.Id).Contains(oi.OrderId));
                    _context.OrderItems.RemoveRange(orderItems);
                    _context.Orders.RemoveRange(orders);

                    // Drop any records attached directly to the client's pets
                    if (client.Pets.Any())
                    {
                        var petIds = client.Pets.Select(p => p.Id).ToList();

                        var petApps = _context.AdoptionApplications.Where(a => petIds.Contains(a.PetId));
                        _context.AdoptionApplications.RemoveRange(petApps);

                        var petAppointments = _context.Appointments.Where(a => a.PetId.HasValue && petIds.Contains(a.PetId.Value));
                        _context.Appointments.RemoveRange(petAppointments);

                        _context.Pets.RemoveRange(client.Pets);
                    }

                    _context.ClientProfiles.Remove(client);
                }
            }
            else if (roleToPurge == "Vet")
            {
                var vet = await _context.VetProfiles.FirstOrDefaultAsync(v => v.UserAccountId == userId);
                if (vet != null)
                {
                    var schedules = _context.WorkingDays.Where(w => w.VetProfileId == vet.Id);
                    _context.WorkingDays.RemoveRange(schedules);

                    var appointments = _context.Appointments.Where(a => a.VetProfileId == vet.Id);
                    _context.Appointments.RemoveRange(appointments);

                    var reviews = _context.VetReviews.Where(r => r.VetProfileId == vet.Id);
                    _context.VetReviews.RemoveRange(reviews);

                    _context.VetProfiles.Remove(vet);
                }
            }
            else if (roleToPurge == "Shelter")
            {
                var shelter = await _context.ShelterProfiles.Include(s => s.Pets).FirstOrDefaultAsync(s => s.UserAccountId == userId);
                if (shelter != null)
                {
                    var schedules = _context.WorkingDays.Where(w => w.ShelterProfileId == shelter.Id);
                    _context.WorkingDays.RemoveRange(schedules);

                    var boardings = _context.BoardingRecords.Where(b => b.ShelterProfileId == shelter.Id);
                    _context.BoardingRecords.RemoveRange(boardings);

                    // Drop records pointing to Shelter's internal pets
                    if (shelter.Pets.Any())
                    {
                        var petIds = shelter.Pets.Select(p => p.Id).ToList();

                        var petApps = _context.AdoptionApplications.Where(a => petIds.Contains(a.PetId));
                        _context.AdoptionApplications.RemoveRange(petApps);

                        var petAppointments = _context.Appointments.Where(a => a.PetId.HasValue && petIds.Contains(a.PetId.Value));
                        _context.Appointments.RemoveRange(petAppointments);

                        _context.Pets.RemoveRange(shelter.Pets);
                    }

                    _context.ShelterProfiles.Remove(shelter);
                }
            }
            else if (roleToPurge == "Merchant")
            {
                var merchant = await _context.MerchantProfiles.FirstOrDefaultAsync(m => m.UserAccountId == userId);
                if (merchant != null)
                {
                    var orders = _context.Orders.Where(o => o.MerchantProfileId == merchant.Id);
                    var orderItems = _context.OrderItems.Where(oi => orders.Select(o => o.Id).Contains(oi.OrderId));
                    _context.OrderItems.RemoveRange(orderItems);
                    _context.Orders.RemoveRange(orders);

                    var products = _context.Products.Where(p => p.MerchantProfileId == merchant.Id);
                    if (products.Any())
                    {
                        var productIds = products.Select(p => p.Id).ToList();

                        var carts = _context.ShoppingCarts.Where(c => productIds.Contains(c.ProductId));
                        _context.ShoppingCarts.RemoveRange(carts);

                        var specificCartItems = _context.CartItems.Where(c => productIds.Contains(c.ProductId));
                        _context.CartItems.RemoveRange(specificCartItems);

                        var reviews = _context.ProductReviews.Where(pr => productIds.Contains(pr.ProductId));
                        _context.ProductReviews.RemoveRange(reviews);

                        _context.Products.RemoveRange(products);
                    }

                    _context.MerchantProfiles.Remove(merchant);
                }
            }
        }
    }
}