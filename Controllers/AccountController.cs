using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pets_friends.Data.ViewModels;
using Pets_friends.Models;
using System.Net;
using System.Net.Mail;

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

        // 1. Show the Forgot Password Page
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // 2. Handle the Submitted Email
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResetLink = Url.Action("ResetPassword", "Account",
                        new { email = model.Email, token = token }, Request.Scheme);

                    try
                    {
                        var smtpClient = new SmtpClient("smtp.gmail.com")
                        {
                            Port = 587,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential("petfriends.support@gmail.com", "aqbsztvthhmqtxyh"),
                            EnableSsl = true
                        };

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress("petfriends.support@gmail.com", "Pet Friends Support"),
                            Subject = "Reset Your Pet Friends Password",
                            Body = $@"
        <div style=""background-color: #f8f9fa; padding: 50px 20px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
            <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05);"">
                
                <div style=""background-color: #5C3D1E; padding: 30px; text-align: center;"">
                    <h1 style=""color: #ffffff; margin: 0; font-size: 24px; letter-spacing: 1px;"">Pet Friends</h1>
                </div>

                <div style=""padding: 40px; text-align: center;"">
                    <h2 style=""color: #333333; margin-top: 0;"">Password Reset Request</h2>
                    <p style=""color: #666666; font-size: 16px; line-height: 1.6;"">
                        Hello! We received a request to reset the password for your Pet Friends account. 
                        No worries—it happens to the best of us! Click the button below to choose a new one.
                    </p>
                    
                    <div style=""margin: 35px 0;"">
                        <a href=""{passwordResetLink}"" 
                           style=""background-color: #C8A882; color: white; padding: 16px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px; display: inline-block; transition: background-color 0.3s;"">
                            Reset Password
                        </a>
                    </div>

                    <p style=""color: #999999; font-size: 13px;"">
                        This link will expire in 2 hours for your security.
                    </p>
                </div>

                <div style=""background-color: #f1f1f1; padding: 20px; text-align: center; border-top: 1px solid #eeeeee;"">
                    <p style=""color: #999999; font-size: 12px; margin: 0;"">
                        If you didn't request this email, you can safely ignore it. Your password won't change until you create a new one.
                    </p>
                    <p style=""color: #999999; font-size: 12px; margin-top: 10px;"">
                        &copy; 2026 Pet Friends Team
                    </p>
                </div>
            </div>
        </div>",
                            IsBodyHtml = true,
                        };
                        mailMessage.To.Add(model.Email);

                        await smtpClient.SendMailAsync(mailMessage);

                        // --- CHANGE HERE ---
                        // We pass the email to the next page so we know the code found the user!
                        TempData["SentToEmail"] = model.Email;
                        return View("ForgotPasswordConfirmation");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "GMAIL ERROR: " + ex.Message);
                        return View(model);
                    }
                }
                else
                {
                    // If the user isn't found, we stay on the page and show this:
                    ModelState.AddModelError("", "We couldn't find an account with that email address.");
                    return View(model);
                }
            }
            return View(model);
        }       
        // 3. Show the Reset Password Page (when they click the email link)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token.");
            }
            return View(new ResetPasswordVM { Token = token, Email = email });
        }

        // 4. Handle the New Password Submission
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return View("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return View("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
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