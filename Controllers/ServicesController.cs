using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data; // Make sure this points to your DbContext namespace

namespace Pets_friends.Controllers
{
    public class ServicesController : Controller
    {
        private readonly AppDbContext _context;

        // Inject the database context via the constructor
        public ServicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Services
        public async Task<IActionResult> Index()
        {
            // Pull the services dynamically from the database instead of a hardcoded list
            var allServices = await _context.Services.ToListAsync();
            return View(allServices);
        }

        // GET: /Services/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var selectedService = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);

            if (selectedService == null)
            {
                return NotFound();
            }

            // NEW CODE: Find any Vets whose profile contains this service name
            var matchingVets = await _context.VetProfiles
                .Include(v => v.UserAccount)
                .Where(v => v.Services != null && v.Services.Contains(selectedService.Name))
                .Take(4) // Let's just show a max of 4 on this page to keep it clean
                .ToListAsync();

            // Send those vets to the HTML View
            ViewBag.MatchingVets = matchingVets;

            return View(selectedService);
        }

            
    }
}