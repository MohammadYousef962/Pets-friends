using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pets_friends.Data.ViewModels;
using Pets_friends.Models;

namespace Pets_friends.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;

        public AccountController(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // REGISTER GET
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // REGISTER POST
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (existingUser != null)
            {
                ModelState.AddModelError("EmailAddress", "This email is already registered.");
                return View(model);
            }

            // Create user
            var user = new UserAccount
            {
                FullName = model.FullName,
                Email = model.EmailAddress,
                UserName = model.EmailAddress
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign default role = User
                var roleResult = await _userManager.AddToRoleAsync(user, "User");

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
                TempData["SuccessMessage"] = "Account created successfully! Please login.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // LOGIN GET
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN POST
        [HttpPost]
        // LOGIN POST
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.EmailAddress,
                model.Password,
                false,
                false);

            if (result.Succeeded)
            {
                // 1. Get the user object based on the email they just logged in with
                var user = await _userManager.FindByEmailAsync(model.EmailAddress);

                // 2. Check what roles they have
                var roles = await _userManager.GetRolesAsync(user);

                // 3. Traffic Cop: Redirect based on their role
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin"); // Send to Admin Dashboard
                }
                else if (roles.Contains("Vet"))
                {
                    return RedirectToAction("Dashboard", "Vet"); // Send Vet to their Profile Manager
                }
                else if (roles.Contains("Merchant"))
                {
                    // Update this later when you build the Merchant Controller!
                    return RedirectToAction("Dashboard", "Merchant");
                }
                else if (roles.Contains("Shelter"))
                {
                    // Update this later when you build the Shelter Controller!
                    return RedirectToAction("Dashboard", "Shelter");
                }

                // 4. Default: If they are just a "User" or "Client", send them to the Home Page
                return RedirectToAction("Dashboard", "Client");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            // 1. Find who is currently logged in
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // 2. Load their existing data into the ViewModel so the form isn't empty
            var model = new EditProfileVM
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                City = user.City
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // 3. Update the user object with the new data from the form
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.City = model.City;

            // 4. Save to Database!
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index", "Home"); // Or wherever you want them to go
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
        // LOGOUT
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            // You can customize this later, but for now it safely redirects 
            // unauthorized snoopers back to the home page with an error message.
            TempData["ErrorMessage"] = "You do not have permission to view that page.";
            return RedirectToAction("Dashboard", "Home");
        }
    }
}