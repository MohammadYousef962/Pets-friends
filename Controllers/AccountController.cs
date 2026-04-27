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

        #region --- REGISTRATION & AUTHENTICATION ---

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var existingUser = await _userManager.FindByEmailAsync(model.EmailAddress);
            if (existingUser != null)
            {
                ModelState.AddModelError("EmailAddress", "This email is already registered.");
                return View(model);
            }

            var user = new UserAccount
            {
                FullName = model.FullName,
                Email = model.EmailAddress,
                UserName = model.EmailAddress
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.EmailAddress, model.Password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.EmailAddress);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin")) return RedirectToAction("Dashboard", "Admin");
                if (roles.Contains("Vet")) return RedirectToAction("Dashboard", "Vet");
                if (roles.Contains("Merchant")) return RedirectToAction("Dashboard", "Merchant");
                if (roles.Contains("Shelter")) return RedirectToAction("Dashboard", "Shelter");

                return RedirectToAction("Dashboard", "Client");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        #endregion

        #region --- PASSWORD MANAGEMENT ---

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResetLink = Url.Action("ResetPassword", "Account", new { email = model.Email, token = token }, Request.Scheme);

                try
                {
                    await SendPasswordResetEmailAsync(model.Email, passwordResetLink);
                    TempData["SentToEmail"] = model.Email;
                    return View("ForgotPasswordConfirmation");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "GMAIL ERROR: " + ex.Message);
                    return View(model);
                }
            }

            // We couldn't find the user
            ModelState.AddModelError("", "We couldn't find an account with that email address.");
            return View(model);
        }

        // 3. Show the Reset Password Page (when they click the email link)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("TokenExpired");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                // Verify the token BEFORE they even try to type a password
                var isValid = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token);

                if (!isValid)
                {
                    // If it's over the time limit, send them to the custom expired page!
                    return RedirectToAction("TokenExpired");
                }
            }
            else
            {
                // If the user doesn't exist, also pretend it's expired for security
                return RedirectToAction("TokenExpired");
            }

            return View(new ResetPasswordVM { Token = token, Email = email });
        }

        // --- NEW: Custom Expired Token Page ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult TokenExpired()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return View("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded) return View("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        #endregion

        #region --- PROFILE & ACCESS MANAGEMENT ---

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

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

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.City = model.City;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "You do not have permission to view that page.";
            return RedirectToAction("Dashboard", "Home");
        }

        #endregion

        #region --- PRIVATE HELPER METHODS ---

        private async Task SendPasswordResetEmailAsync(string email, string passwordResetLink)
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

            mailMessage.To.Add(email);
            await smtpClient.SendMailAsync(mailMessage);
        }

        #endregion
    }
}