// Controllers/VetController.cs
// Graduation Project: Pets-friends
// ---------------------------------------------------------------------------
// MENTOR NOTE:
//   Right now this controller manually builds a VetProfile object (dummy data).
//   When you integrate EF Core, replace the dummy block inside Profile() with:
//       var vet = await _context.VetProfiles
//                               .Include(v => v.Reviews)
//                               .FirstOrDefaultAsync(v => v.Id == id);
//   Then inject your DbContext via the constructor.
// ---------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Pets_friends.Controllers;
using Pets_friends.Models;
namespace Pets_friends.Controllers
{
    public class VetController : Controller
    {
        // ── Future: inject DbContext here ────────────────────────────────────
        // private readonly AppDbContext _context;
        // public VetController(AppDbContext context) { _context = context; }

        // GET /Vet/Profile/{id}
        public IActionResult Profile(int id = 1)
        {
            // ── DUMMY DATA – replace this block with a DB call later ──────────
            var vet = new VetProfile
            {
                Id = 1,
                FullName = "Dr. Sarah Al-Khalidi",
                Title = "Doctor of Veterinary Medicine",
                PhotoUrl = "https://placehold.co/160x160/C8A882/white?text=Dr.Sarah",
                Specialization = "Small Animal & Exotic Pet Specialist",
                ClinicName = "PetPals Veterinary Center",
                ClinicAddress = "12 Al-Madeena St., Amman, Jordan",
                YearsOfExperience = 9,
                Bio = "Dr. Sarah is a compassionate veterinarian with over 9 years of experience " +
                                    "caring for dogs, cats, rabbits, and exotic companions. She graduated top of " +
                                    "her class from Jordan University of Science & Technology and completed her " +
                                    "residency at the Royal Veterinary College, London. Her philosophy is simple: " +
                                    "treat every patient as if it were her own.",
                Email = "sarah.khalidi@petpals.jo",
                Phone = "+962 79 123 4567",
                AverageRating = 4.8,
                TotalReviews = 127,
                HappyPatients = 1400,

                Services = new List<string>
                {
                    "General Check-ups & Vaccinations",
                    "Dental Cleaning & Oral Health",
                    "Spay & Neuter Surgery",
                    "Dermatology & Allergy Treatment",
                    "Exotic Pet Care (rabbits, birds, reptiles)",
                    "Emergency & Critical Care",
                    "Nutrition & Weight Management",
                    "Post-Surgery Rehabilitation"
                },

                Education = new List<EducationEntry>
                {
                    new() { Degree = "Doctor of Veterinary Medicine (DVM)",  Institution = "Jordan University of Science & Technology", Year = 2015 },
                    new() { Degree = "Residency – Small Animal Medicine",     Institution = "Royal Veterinary College, London",           Year = 2017 },
                    new() { Degree = "Certificate in Exotic Animal Practice", Institution = "University of Edinburgh",                    Year = 2019 }
                },

                Certifications = new List<string>
                {
                    "RCVS Registered Veterinary Surgeon",
                    "AVMA Member",
                    "Certified Fear-Free Practitioner",
                    "Advanced Cardiac Ultrasound – Level II"
                },

                WorkingHours = new List<WorkingHoursEntry>
                {
                    new() { Day = "Sunday",    Hours = "9:00 AM – 5:00 PM"  },
                    new() { Day = "Monday",    Hours = "9:00 AM – 5:00 PM"  },
                    new() { Day = "Tuesday",   Hours = "9:00 AM – 5:00 PM"  },
                    new() { Day = "Wednesday", Hours = "9:00 AM – 1:00 PM"  },
                    new() { Day = "Thursday",  Hours = "9:00 AM – 5:00 PM"  },
                    new() { Day = "Friday",    Hours = "Closed", IsOff = true },
                    new() { Day = "Saturday",  Hours = "10:00 AM – 3:00 PM" }
                },

                Reviews = new List<ReviewEntry>
                {
                    new()
                    {
                        AuthorName = "Lina M.",
AvatarUrl = "https://placehold.co/42x42/C8A882/white?text=L",               
                        Rating     = 5,
                        Comment    = "Dr. Sarah is absolutely wonderful! She treated our anxious golden retriever " +
                                     "with so much patience and care. We will not go anywhere else.",
                        Date       = new DateTime(2025, 3, 14),
                        PetType    = "Dog"
                    },
                    new()
                    {
                        AuthorName = "Omar T.",
                        AvatarUrl = "https://placehold.co/42x42/C8A882/white?text=O",
                        Rating     = 5,
                        Comment    = "Took my rabbit for a check-up and was impressed by how knowledgeable " +
                                     "she is with exotic pets. Highly recommend for non-standard animals!",
                        Date       = new DateTime(2025, 2, 28),
                        PetType    = "Rabbit"
                    },
                    new()
                    {
                        AuthorName = "Reem A.",
                        AvatarUrl = "https://placehold.co/42x42/C8A882/white?text=R",
                        Rating     = 4,
                        Comment    = "Very professional and thorough. The clinic is clean and the staff are " +
                                     "friendly. Waiting time can be a bit long on busy days.",
                        Date       = new DateTime(2025, 1, 9),
                        PetType    = "Cat"
                    }
                }
            };

            return View(vet);
        }
    }
}