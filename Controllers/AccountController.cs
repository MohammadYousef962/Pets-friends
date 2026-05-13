using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pets_friends.Data;
using Pets_friends.Data.ViewModels;
using Pets_friends.Models;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;


namespace Pets_friends.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // ADD THIS
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly IConfiguration _configuration; // <-- Added this

        // Inject IConfiguration AND IWebHostEnvironment into the constructor
        public AccountController(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment) // <-- Added this parameter!
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment; // <-- Assigned it here!
        }

        #region --- REGISTRATION & AUTHENTICATION ---

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            // Bounce authenticated users away from the registration form
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Admin")) return RedirectToAction("Dashboard", "Admin");
                    if (roles.Contains("Vet")) return RedirectToAction("Dashboard", "Vet");
                    if (roles.Contains("Merchant")) return RedirectToAction("Dashboard", "Merchant");
                    if (roles.Contains("Shelter")) return RedirectToAction("Dashboard", "Shelter");

                    return RedirectToAction("Dashboard", "Client");
                }
            }

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
                // Change from "User" to "Client" - this role actually exists
                var roleResult = await _userManager.AddToRoleAsync(user, "Client");
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
        public async Task<IActionResult> Login()
        {
            // 1. Check if the user's browser already has a valid login cookie
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // 2. If they are logged in, grab their info and roles
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // 3. Immediately redirect them to their correct dashboard!
                    if (roles.Contains("Admin")) return RedirectToAction("Dashboard", "Admin");
                    if (roles.Contains("Vet")) return RedirectToAction("Dashboard", "Vet");
                    if (roles.Contains("Merchant")) return RedirectToAction("Dashboard", "Merchant");
                    if (roles.Contains("Shelter")) return RedirectToAction("Dashboard", "Shelter");

                    return RedirectToAction("Dashboard", "Client");
                }
            }

            // 4. If they are NOT logged in, show the standard login page
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid) return View(model);

            // CHANGED: The 3rd parameter is now 'true' to set a persistent cookie.
            // This keeps them logged in even if they close the browser.
            var result = await _signInManager.PasswordSignInAsync(model.EmailAddress, model.Password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.EmailAddress);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin")) return RedirectToAction("Dashboard", "Admin");
                if (roles.Contains("Vet")) return RedirectToAction("Dashboard", "Vet");
                if (roles.Contains("Merchant")) return RedirectToAction("Dashboard", "Merchant");
                if (roles.Contains("Shelter")) return RedirectToAction("Dashboard", "Shelter");

                return RedirectToAction("Dashboard", "Client"); // Assuming Client is the default
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
                var isValid = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token);

                if (!isValid)
                {
                    return RedirectToAction("TokenExpired");
                }
            }
            else
            {
                return RedirectToAction("TokenExpired");
            }

            return View(new ResetPasswordVM { Token = token, Email = email });
        }

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

            // 1. Update text fields
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.City = model.City;

            // 2. 📸 HANDLE THE PHOTO UPLOAD
            if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
            {
                // Navigate to wwwroot/images/profiles
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");

                // Create the folder if it doesn't exist yet
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Create a unique file name (e.g., "b4a3c1_photo.jpg")
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfilePhoto.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the physical file to the server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePhoto.CopyToAsync(fileStream);
                }

                // Save the URL path to the database
                user.AvatarUrl = "/images/profiles/" + uniqueFileName;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                // Bounce them back to their Dashboard so they can instantly see their new photo!
                return RedirectToAction("Dashboard", "Client");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AccessDenied(string returnUrl = null)
        {
            // Safely assigns the referring page if supplied
            ViewData["ReturnUrl"] = returnUrl;

            string dashboardUrl = "~/";

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Admin")) dashboardUrl = "~/Admin/Dashboard";
                    else if (roles.Contains("Vet")) dashboardUrl = "~/Vet/Dashboard";
                    else if (roles.Contains("Merchant")) dashboardUrl = "~/Merchant/Dashboard";
                    else if (roles.Contains("Shelter")) dashboardUrl = "~/Shelter/Dashboard";
                    else dashboardUrl = "~/Client/Dashboard";
                }
            }

            ViewData["DashboardUrl"] = dashboardUrl;
            return View();
        }
        #endregion

        #region --- PRIVATE HELPER METHODS ---

        private async Task SendPasswordResetEmailAsync(string email, string passwordResetLink)
        {
            // Read settings securely from appsettings.json
            string smtpServer = _configuration["EmailSettings:SmtpServer"];
            int smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            string senderEmail = _configuration["EmailSettings:SenderEmail"];
            string senderName = _configuration["EmailSettings:SenderName"];
            string appPassword = _configuration["EmailSettings:AppPassword"];

            // Dynamic year for the footer
            string currentYear = DateTime.Now.Year.ToString();

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail, appPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
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
                        &copy; {currentYear} Pet Friends Team
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