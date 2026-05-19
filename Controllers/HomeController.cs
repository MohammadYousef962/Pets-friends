using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Data.ViewModels;
using Pets_friends.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pets_friends.Controllers
{
    [AllowAnonymous] // Allows guests to view the home pages
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        public HomeController(AppDbContext context, UserManager<UserAccount> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --------------------------------------------------------
        // HOME / MAIN PAGE
        // --------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Main()
        {
            var viewModel = new HomeVM();

            // 1. Fetch Adoptable Pets (Ensuring they belong to a shelter!)
            viewModel.AdoptablePets = await _context.Pets
                .Include(p => p.ShelterProfile)
                .ThenInclude(s => s.UserAccount)
                .Where(p => p.IsPubliclyListed && !p.IsAdopted && p.ShelterProfileId != null)
                .Select(p => new HomePetDto
                {
                    Id = p.Id,
                    Name = p.Name ?? "Friend",
                    Breed = p.Breed ?? "Mixed Breed",
                    Age = p.Age,
                    ImageUrl = p.ImageUrl ?? "https://placehold.co/400x400/FAF6F1/5C3D1E?text=Pet",
                    ShelterName = p.ShelterProfile.UserAccount != null ? p.ShelterProfile.UserAccount.FullName : "Verified Shelter",
                    Gender = p.Gender ?? "Unknown",
                    IsNeutered = p.IsNeutered,
                    MedicalHistory = p.MedicalHistory ?? "Verified healthy and up to date on all shots.",
                    Description = p.Description ?? "Friendly companion animal looking for a loving home."
                })
                .ToListAsync();

            // 2. Fetch Top Clinics
            viewModel.TopClinics = await _context.VetProfiles
                .Include(v => v.Reviews)
                .OrderByDescending(v => v.Reviews.Any() ? v.Reviews.Average(r => r.Rating) : 0)
                .Take(3)
                .Select(v => new HomeVetDto
                {
                    Id = v.Id,
                    ClinicName = v.ClinicName ?? "Unnamed Clinic",
                    Specialties = v.Specialization ?? "General Practice",
                    ImageUrl = v.ImageUrl ?? "https://placehold.co/150x150/C8A882/white?text=Vet",
                    Rating = v.Reviews.Any() ? Math.Round(v.Reviews.Average(r => r.Rating), 1) : 0.0,
                    ReviewCount = v.Reviews.Count,
                    RecentReview = v.Reviews.OrderByDescending(r => r.Id).Select(r => r.Comment).FirstOrDefault() ?? "Highly recommended by the community."
                })
                .ToListAsync();

            // 3. Fetch Featured Products
            viewModel.FeaturedProducts = await _context.Products
                .Include(p => p.Reviews)
                .Include(p => p.MerchantProfile)
                .ThenInclude(m => m.UserAccount)
                .OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0)
                .Take(3)
                .Select(p => new HomeProductDto
                {
                    Id = p.Id,
                    Title = p.Name,
                    StoreName = p.MerchantProfile.StoreName ?? "Marketplace Partner",
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Rating = p.Reviews.Any() ? Math.Round(p.Reviews.Average(r => r.Rating), 1) : 0.0,
                    ReviewCount = p.Reviews.Count
                })
                .ToListAsync();

            // 4. Calculate the OVERALL average and total count for ALL Vets
            var allVetReviews = await _context.VetProfiles.SelectMany(v => v.Reviews).ToListAsync();
            if (allVetReviews.Any())
            {
                viewModel.OverallVetAverageRating = Math.Round(allVetReviews.Average(r => r.Rating), 1);
                viewModel.TotalVetReviews = allVetReviews.Count;
            }
            else
            {
                viewModel.OverallVetAverageRating = 5.0;
                viewModel.TotalVetReviews = 0;
            }

            // 5. Query the 2 MOST RECENT Reviews globally from Vets
            var recentReviews = await _context.VetProfiles
                .Include(v => v.Reviews)
                .ThenInclude(r => r.Reviewer)
                .SelectMany(v => v.Reviews)
                .OrderByDescending(r => r.Id)
                .Take(2)
                .Select(r => new TestimonialDto
                {
                    Comment = r.Comment,
                    AuthorName = r.Reviewer != null ? r.Reviewer.FullName : "Anonymous Client",
                    Role = "Verified Vet Care Client",
                    Rating = r.Rating
                })
                .ToListAsync();

            if (recentReviews.Count < 1)
                recentReviews.Add(new TestimonialDto { Comment = "Booking verification appointments and looking up certified pet history records is incredibly easy.", AuthorName = "Sarah Jenkins", Role = "Verified Vet Care Client", Rating = 5 });
            if (recentReviews.Count < 2)
                recentReviews.Add(new TestimonialDto { Comment = "The platform saved me so much time. I found a great local vet in minutes!", AuthorName = "Michael Chen", Role = "Verified Pet Parent", Rating = 5 });

            viewModel.RecentTestimonials = recentReviews;

            return View(viewModel);
        }

        // --------------------------------------------------------
        // AVAILABLE PETS DIRECTORY
        // --------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> AvailablePets(int page = 1)
        {
            int pageSize = 16;

            // Ensuring they belong to a shelter!
            var query = _context.Pets
                .Include(p => p.ShelterProfile)
                .ThenInclude(s => s.UserAccount)
                .Where(p => p.IsPubliclyListed && !p.IsAdopted && p.ShelterProfileId != null);

            int totalPets = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalPets / (double)pageSize);
            if (totalPages == 0) totalPages = 1;

            var pets = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new GlobalPetDto
                {
                    PetId = p.Id,
                    Name = p.Name,
                    Breed = p.Breed,
                    Age = p.Age,
                    ImageUrl = p.ImageUrl ?? "https://placehold.co/400x400/FAF6F1/5C3D1E?text=Pet",
                    ShelterName = p.ShelterProfile.UserAccount != null ? p.ShelterProfile.UserAccount.FullName : "Shelter",
                    Gender = p.Gender,
                    IsNeutered = p.IsNeutered,
                    MedicalHistory = p.MedicalHistory ?? "No records.",
                    Description = p.Description ?? "A lovely pet."
                })
                .ToListAsync();

            var vm = new GlobalPetsVM
            {
                Pets = pets,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalPets = totalPets
            };

            return View(vm);
        }

        // --------------------------------------------------------
        // SUBMIT ADOPTION REQUEST (Requires Login!)
        // --------------------------------------------------------
        [HttpPost]
        [Authorize] // Forces the user to log in before executing this action
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAdoption(int PetId, bool AgreedToPolicy)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var client = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);
            if (client == null)
            {
                client = new ClientProfile { UserAccountId = user.Id };
                _context.ClientProfiles.Add(client);
                await _context.SaveChangesAsync();
            }

            var pet = await _context.Pets.FindAsync(PetId);
            if (pet == null) return NotFound();

            if (!AgreedToPolicy)
            {
                TempData["ErrorMessage"] = "You must agree to the shelter's post-adoption policy.";
                return RedirectToAction("AvailablePets");
            }

            // Syncs perfectly with the Shelter's Dashboard Queue!
            var app = new AdoptionApplication
            {
                PetId = PetId,
                ClientProfileId = client.Id,
                Type = "Adoption",
                Status = "Pending",
                ApplicationDate = DateTime.Now
            };

            _context.AdoptionApplications.Add(app);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Your adoption application for {pet.Name} has been submitted successfully!";
            return RedirectToAction("AvailablePets");
        }

        // --------------------------------------------------------
        // VET CLINICS DIRECTORY
        // --------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Clinics(string query, string category, int page = 1)
        {
            var clinicsQuery = _context.VetProfiles.Include(v => v.Reviews).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var term = $"%{query.Trim()}%";
                clinicsQuery = clinicsQuery.Where(v =>
                    (v.ClinicName != null && EF.Functions.Like(v.ClinicName, term)) ||
                    (v.ClinicAddress != null && EF.Functions.Like(v.ClinicAddress, term)) ||
                    (v.Specialization != null && EF.Functions.Like(v.Specialization, term)));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "all")
            {
                var catTerm = $"%{category.Trim()}%";
                clinicsQuery = clinicsQuery.Where(v =>
                    v.Specialization != null && EF.Functions.Like(v.Specialization, catTerm));
            }

            int pageSize = 6;
            int totalItems = await clinicsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var pagedClinics = await clinicsQuery
                .OrderByDescending(v => v.Reviews.Any() ? v.Reviews.Average(r => r.Rating) : 0)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VetClinicDto
                {
                    Id = v.Id,
                    ClinicName = v.ClinicName ?? "Unnamed Clinic",
                    Location = v.ClinicAddress ?? "Amman, Jordan",
                    Specialties = v.Specialization ?? "General Practice",
                    ImageUrl = v.ImageUrl ?? "/images/default-vet.png",
                    AverageRating = v.Reviews.Any() ? Math.Round(v.Reviews.Average(r => r.Rating), 1) : 0.0,
                    ReviewCount = v.Reviews.Count
                })
                .ToListAsync();

            var viewModel = new ClinicsDirectoryVM
            {
                Clinics = pagedClinics,
                CurrentQuery = query ?? string.Empty,
                CurrentCategory = category ?? "all",
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Shelters(string query = "", int page = 1)
        {
            int pageSize = 6;
            var sheltersQuery = _context.ShelterProfiles
                .Include(s => s.UserAccount)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                sheltersQuery = sheltersQuery.Where(s =>
                    (s.UserAccount != null && s.UserAccount.FullName.ToLower().Contains(query)) ||
                    (s.Address != null && s.Address.ToLower().Contains(query)) ||
                    (s.Description != null && s.Description.ToLower().Contains(query))
                );
            }

            int totalShelters = await sheltersQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalShelters / (double)pageSize);

            var sheltersList = await sheltersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ShelterDisplayVM
                {
                    Id = s.Id,
                    Name = s.UserAccount != null ? s.UserAccount.FullName : "Shelter",
                    Location = !string.IsNullOrWhiteSpace(s.Address) ? s.Address : "Location not provided",
                    Description = !string.IsNullOrWhiteSpace(s.Description) ? s.Description : "A loving animal rescue and shelter.",
                    ImageUrl = !string.IsNullOrWhiteSpace(s.ImageUrl) ? s.ImageUrl : "https://placehold.co/150x150/C8A882/white?text=Shelter"
                })
                .ToListAsync();

            var vm = new SheltersDirectoryVM
            {
                Shelters = sheltersList,
                CurrentQuery = query,
                CurrentPage = page,
                TotalPages = totalPages == 0 ? 1 : totalPages
            };

            return View(vm);
        }
    }
}