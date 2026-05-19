using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Data.ViewModels;
using Pets_friends.Models;

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

        // 1. DASHBOARD - Optimized with single-pass calculation if possible
        public async Task<IActionResult> Dashboard()
        {
            // 1. Gather Gross Revenue from Completed orders
            var storeRevenue = await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);

            var vm = new AdminDashboardVM
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(o => o.Status != "Cancelled"),
                TotalRevenue = storeRevenue,

                // Back-calculates the 8% tax embedded in Completed revenue
                TotalTax = storeRevenue > 0 ? (storeRevenue / 1.08m) * 0.08m : 0m,

                // RECONCILED: Counts all appointments actively on the books (Excludes Cancelled/No-shows)
                TotalAppointments = await _context.Appointments
                    .CountAsync(a => a.Status != "Cancelled" && a.Status != "No-Show")
            };

            return View(vm);
        }

        // 2. TRANSACTION HISTORY - Optimized with Projections (.Select)
        public async Task<IActionResult> Transactions()
        {
            var orders = await _context.Orders
                .Select(o => new TransactionRecordVM
                {
                    ReferenceId = o.Id,
                    Timestamp = o.OrderDate,
                    Type = "Product",
                    CustomerName = o.ClientProfile.UserAccount.FullName,
                    Details = "Store Purchase",
                    Amount = o.TotalAmount,
                    Status = o.Status
                }).ToListAsync();

            var appointments = await _context.Appointments
                .Select(a => new TransactionRecordVM
                {
                    ReferenceId = a.Id,
                    Timestamp = a.AppointmentDate,
                    Type = "Service",
                    CustomerName = a.ClientProfile.UserAccount.FullName,
                    Details = a.Service.Name + " for " + a.Pet.Name,
                    Amount = null,
                    Status = a.Status
                }).ToListAsync();

            var allTransactions = orders.Concat(appointments)
                .OrderByDescending(t => t.Timestamp)
                .ToList();

            return View(allTransactions);
        }

        // 3. MANAGE ACCOUNTS - Fixed N+1 Problem
        // 3. MANAGE ACCOUNTS - Fixed N+1 Problem
        public async Task<IActionResult> ManageUsers()
        {
            var userVMs = await (from user in _context.Users
                                 join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                 join role in _context.Roles on userRole.RoleId equals role.Id
                                 select new ManageUserVM
                                 {
                                     UserId = user.Id,
                                     FullName = user.FullName,
                                     Email = user.Email,
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

            // Prevent self-demotion or removing the last admin
            if (currentRoles.Contains("Admin") && newRole != "Admin")
            {
                TempData["ErrorMessage"] = "Safety check: Cannot demote an Admin via this panel.";
                return RedirectToAction(nameof(ManageUsers));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Change the role in Identity
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);

                // 2. PURGE CONTRA-ROLE DATA
                if (newRole != "Client")
                {
                    var client = await _context.ClientProfiles.Include(c => c.Pets).FirstOrDefaultAsync(c => c.UserAccountId == userId);
                    if (client != null)
                    {
                        // FIX: Instead of setting to null, we delete the orders and their line items cleanly
                        var clientOrders = _context.Orders.Where(o => o.ClientProfileId == client.Id);
                        var clientOrderItems = _context.OrderItems.Where(oi => clientOrders.Select(o => o.Id).Contains(oi.OrderId));

                        _context.OrderItems.RemoveRange(clientOrderItems);
                        _context.Orders.RemoveRange(clientOrders);

                        _context.Appointments.RemoveRange(_context.Appointments.Where(a => a.ClientProfileId == client.Id));
                        _context.VetReviews.RemoveRange(_context.VetReviews.Where(r => r.ReviewerId == userId));
                        _context.RemoveRange(client.Pets);
                        _context.ClientProfiles.Remove(client);
                    }
                }
                if (newRole != "Vet")
                {
                    var vet = await _context.VetProfiles.Include(v => v.Schedule).FirstOrDefaultAsync(v => v.UserAccountId == userId);
                    if (vet != null)
                    {
                        _context.Appointments.RemoveRange(_context.Appointments.Where(a => a.VetProfileId == vet.Id));
                        _context.RemoveRange(vet.Schedule);
                        _context.VetProfiles.Remove(vet);
                    }
                }
                if (newRole != "Merchant")
                {
                    var merchant = await _context.MerchantProfiles.FirstOrDefaultAsync(m => m.UserAccountId == userId);
                    if (merchant != null)
                    {
                        // FIX: Remove merchant store orders and dependent items instead of nullifying
                        var merchantOrders = _context.Orders.Where(o => o.MerchantProfileId == merchant.Id);
                        var merchantOrderItems = _context.OrderItems.Where(oi => merchantOrders.Select(o => o.Id).Contains(oi.OrderId));

                        _context.OrderItems.RemoveRange(merchantOrderItems);
                        _context.Orders.RemoveRange(merchantOrders);
                        _context.MerchantProfiles.Remove(merchant);
                    }
                }
                if (newRole != "Shelter")
                {
                    var shelter = await _context.ShelterProfiles.FirstOrDefaultAsync(s => s.UserAccountId == userId);
                    if (shelter != null) _context.ShelterProfiles.Remove(shelter);
                }

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

        // 4. DELETE ACCOUNT - Re-engineered to handle Non-Nullable Foregin Keys
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
                // 1. Clear shopping carts
                var carts = _context.ShoppingCarts.Where(c => c.UserAccountId == userId);
                _context.ShoppingCarts.RemoveRange(carts);

                // 2. Clear client profiles, appointments, and orders cascadingly
                var client = await _context.ClientProfiles.Include(c => c.Pets).FirstOrDefaultAsync(c => c.UserAccountId == userId);
                if (client != null)
                {
                    var clientAppointments = _context.Appointments.Where(a => a.ClientProfileId == client.Id);
                    _context.Appointments.RemoveRange(clientAppointments);

                    // FIX: Safely extract and purge associated Order Items first, then the Orders
                    var clientOrders = _context.Orders.Where(o => o.ClientProfileId == client.Id);
                    var clientOrderItems = _context.OrderItems.Where(oi => clientOrders.Select(o => o.Id).Contains(oi.OrderId));

                    _context.OrderItems.RemoveRange(clientOrderItems);
                    _context.Orders.RemoveRange(clientOrders);

                    _context.VetReviews.RemoveRange(_context.VetReviews.Where(r => r.ReviewerId == userId));
                    _context.RemoveRange(client.Pets);
                    _context.ClientProfiles.Remove(client);
                }

                // 3. Clear vet profiles
                var vet = await _context.VetProfiles.Include(v => v.Schedule).FirstOrDefaultAsync(v => v.UserAccountId == userId);
                if (vet != null)
                {
                    var vetAppointments = _context.Appointments.Where(a => a.VetProfileId == vet.Id);
                    _context.Appointments.RemoveRange(vetAppointments);
                    _context.RemoveRange(vet.Schedule);
                    _context.VetProfiles.Remove(vet);
                }

                // 4. Clear merchant profiles and storefront orders
                var merchant = await _context.MerchantProfiles.FirstOrDefaultAsync(m => m.UserAccountId == userId);
                if (merchant != null)
                {
                    var merchantOrders = _context.Orders.Where(o => o.MerchantProfileId == merchant.Id);
                    var merchantOrderItems = _context.OrderItems.Where(oi => merchantOrders.Select(o => o.Id).Contains(oi.OrderId));

                    _context.OrderItems.RemoveRange(merchantOrderItems);
                    _context.Orders.RemoveRange(merchantOrders);
                    _context.MerchantProfiles.Remove(merchant);
                }

                // 5. Clear shelter profiles
                var shelter = await _context.ShelterProfiles.FirstOrDefaultAsync(s => s.UserAccountId == userId);
                if (shelter != null) _context.ShelterProfiles.Remove(shelter);

                await _context.SaveChangesAsync();

                // 6. Delete identity user account
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
    }
}