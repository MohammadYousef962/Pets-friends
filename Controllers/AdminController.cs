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
        // 4. DELETE ACCOUNT (Updated with Profile Cascade Deletion)
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (user.Id == loggedInUser.Id)
            {
                TempData["ErrorMessage"] = "Safety feature: You cannot delete your own Admin account.";
                return RedirectToAction(nameof(ManageUsers));
            }

            try
            {
                // 1. WIPE CLIENT DATA (Pets, Appointments, Reviews)
                var clientProfile = await _context.ClientProfiles
                    .Include(c => c.Pets) // Assuming a client has Pets
                    .FirstOrDefaultAsync(c => c.UserAccountId == userId);

                if (clientProfile != null)
                {
                    // Find and delete all appointments for this client
                    var appointments = await _context.Appointments.Where(a => a.ClientProfileId == clientProfile.Id).ToListAsync();
                    _context.Appointments.RemoveRange(appointments);

                    // Find and delete all reviews left by this client
                    var reviews = await _context.VetReviews.Where(r => r.ReviewerId == userId).ToListAsync();
                    _context.VetReviews.RemoveRange(reviews);

                    // Remove the pets and the profile
                    if (clientProfile.Pets != null) _context.RemoveRange(clientProfile.Pets);
                    _context.ClientProfiles.Remove(clientProfile);
                }

                // 2. WIPE VET DATA (Schedule, Appointments attached to Vet)
                var vetProfile = await _context.VetProfiles
                    .Include(v => v.Schedule)
                    .FirstOrDefaultAsync(v => v.UserAccountId == userId);

                if (vetProfile != null)
                {
                    var vetAppointments = await _context.Appointments.Where(a => a.VetProfileId == vetProfile.Id).ToListAsync();
                    _context.Appointments.RemoveRange(vetAppointments);

                    if (vetProfile.Schedule != null) _context.RemoveRange(vetProfile.Schedule);
                    _context.VetProfiles.Remove(vetProfile);
                }

                // WIPE MERCHANT/SHELTER PROFILES
                var merchantProfile = await _context.MerchantProfiles.FirstOrDefaultAsync(m => m.UserAccountId == userId);
                if (merchantProfile != null) _context.MerchantProfiles.Remove(merchantProfile);

                var shelterProfile = await _context.ShelterProfiles.FirstOrDefaultAsync(s => s.UserAccountId == userId);
                if (shelterProfile != null) _context.ShelterProfiles.Remove(shelterProfile);

                // Save all the deep deletions
                await _context.SaveChangesAsync();

                // 3. FINALLY, DELETE THE LOGIN ACCOUNT
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"{user.FullName}'s entire account and data was permanently wiped.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Data cleared, but failed to delete Identity account.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Deep clean failed. Details: {ex.InnerException?.Message ?? ex.Message}";
            }

            return RedirectToAction(nameof(ManageUsers));
        }
    }
}