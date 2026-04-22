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

        // ====================================================================
        // 1. DASHBOARD
        // ====================================================================
        public async Task<IActionResult> Dashboard()
        {
            var vm = new AdminDashboardVM
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync(),
                TotalRevenue = await _context.Orders
                                    .Where(o => o.Status == "Completed" || o.Status == "Pending")
                                    .SumAsync(o => o.TotalAmount)
            };

            return View(vm);
        }

        // ====================================================================
        // 2. TRANSACTION HISTORY (New!)
        // ====================================================================
        public async Task<IActionResult> Transactions()
        {
            var transactions = new List<TransactionRecordVM>();

            // A. Fetch all Product Orders
            var orders = await _context.Orders
                .Include(o => o.ClientProfile).ThenInclude(c => c.UserAccount)
                .ToListAsync();

            foreach (var o in orders)
            {
                transactions.Add(new TransactionRecordVM
                {
                    ReferenceId = o.Id,
                    Timestamp = o.OrderDate,
                    Type = "Product Order",
                    CustomerName = o.ClientProfile?.UserAccount?.FullName ?? "Deleted User",
                    Details = "Store Purchase",
                    Amount = o.TotalAmount,
                    Status = o.Status
                });
            }

            // B. Fetch all Service Appointments
            var appointments = await _context.Appointments
                .Include(a => a.ClientProfile).ThenInclude(c => c.UserAccount)
                .Include(a => a.Service)
                .Include(a => a.Pet)
                .ToListAsync();

            foreach (var a in appointments)
            {
                transactions.Add(new TransactionRecordVM
                {
                    ReferenceId = a.Id,
                    Timestamp = a.AppointmentDate,
                    Type = "Service Appointment",
                    CustomerName = a.ClientProfile?.UserAccount?.FullName ?? "Deleted User",
                    Details = $"{a.Service?.Name ?? "Service"} for {a.Pet?.Name ?? "Pet"}",
                    Amount = a.Service?.Price ?? 0, // Fallback to 0 if no price
                    Status = a.Status
                });
            }

            // C. Sort everything chronologically (newest first)
            transactions = transactions.OrderByDescending(t => t.Timestamp).ToList();

            return View(transactions);
        }

        // ====================================================================
        // 3. MANAGE ACCOUNTS 
        // ====================================================================
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userVMs = new List<ManageUserVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userVMs.Add(new ManageUserVM
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    CurrentRole = roles.FirstOrDefault() ?? "Client"
                });
            }

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
                TempData["ErrorMessage"] = "You cannot demote an Admin account.";
                return RedirectToAction(nameof(ManageUsers));
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["SuccessMessage"] = $"{user.FullName} Has been changed to {newRole}";
            return RedirectToAction(nameof(ManageUsers));
        }

        // ====================================================================
        // 4. DELETE ACCOUNT (New!)
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Prevent the admin from deleting themselves
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (user.Id == loggedInUser.Id)
            {
                TempData["ErrorMessage"] = "Safety feature: You cannot delete your own Admin account.";
                return RedirectToAction(nameof(ManageUsers));
            }

            try
            {
                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = $"{user.FullName}'s account was permanently deleted.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Could not delete user. They may have active orders or reviews attached to them.";
            }

            return RedirectToAction(nameof(ManageUsers));
        }
    }
}