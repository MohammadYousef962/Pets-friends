using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pets_friends.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<SearchResult>>> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(new List<SearchResult>());

            var query = q.ToLower();
            var results = new List<SearchResult>();

            // Search Vets
            var vets = await _context.VetProfiles
                .Include(v => v.UserAccount)
                .Where(v => v.ClinicName.ToLower().Contains(query) || 
                           v.UserAccount.FullName.ToLower().Contains(query))
                .Take(5)
                .ToListAsync();

            foreach (var vet in vets)
            {
                results.Add(new SearchResult
                {
                    Name = vet.UserAccount?.FullName ?? vet.ClinicName,
                    Type = "Vet",
                    Url = $"/Vet/Profile/{vet.UserAccountId}"
                });
            }

            // Search Shelters
            var shelters = await _context.ShelterProfiles
                .Include(s => s.UserAccount)
                .Where(s => s.ShelterName.ToLower().Contains(query) || 
                           s.UserAccount.FullName.ToLower().Contains(query))
                .Take(5)
                .ToListAsync();

            foreach (var shelter in shelters)
            {
                results.Add(new SearchResult
                {
                    Name = shelter.ShelterName,
                    Type = "Shelter",
                    Url = $"/Shelter/Profile/{shelter.UserAccountId}"
                });
            }

            return Ok(results.Take(10));
        }
    }

    public class SearchResult
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
    }
}