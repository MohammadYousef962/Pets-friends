using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Data.ViewModels;
using Pets_friends.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Pets_friends.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        // Inject BOTH the database context and the user manager
        public HomeController(AppDbContext context, UserManager<UserAccount> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 1. SMART ROUTING: Check if the browser has an active login cookie
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // 2. Teleport staff directly to their workspaces!
                    if (roles.Contains("Admin")) return RedirectToAction("Dashboard", "Admin");
                    if (roles.Contains("Vet")) return RedirectToAction("Dashboard", "Vet");
                    if (roles.Contains("Merchant")) return RedirectToAction("Dashboard", "Merchant");
                    if (roles.Contains("Shelter")) return RedirectToAction("Dashboard", "Shelter");

                    // let Signed in Clients to automatically go to their 
                    // dashboard instead of the home page 
                    //return RedirectToAction("Dashboard", "Client");
                }
            }

            // 3. PUBLIC USERS ONLY: If they aren't logged in, load the public landing page
            var featuredServices = await _context.Services.Take(6).ToListAsync();

            var vm = new LandingPageViewModel
            {
                FeaturedServices = featuredServices
            };

            return View(vm);
        }

        // GET /Home/About  (stub)
        public IActionResult About() => View();

        // GET /Home/Services (stub)
        public IActionResult Services() => View();
    }
}