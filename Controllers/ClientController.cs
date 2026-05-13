using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Data;
using Pets_friends.Models;
using Pets_friends.Data.ViewModels;
using PetFriends.ViewModels.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pets_friends.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly AppDbContext _context;

        public ClientController(
            UserManager<UserAccount> userManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // ════════════════════════════════════════════════════════════════════════
        // DASHBOARD
        // ════════════════════════════════════════════════════════════════════════

        // GET /Client/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // ── 1. Resolve the current user ──────────────────────────────────────
            var userAccount = await _userManager.GetUserAsync(User);
            if (userAccount == null) return RedirectToAction("Login", "Account");

            // ── 2. Eager-load all related data in a single round-trip ────────────
            var clientProfile = await _context.ClientProfiles
                .Include(cp => cp.UserAccount)
                .Include(cp => cp.Pets)
                    .ThenInclude(p => p.VerifiedByVet)
                        .ThenInclude(v => v.UserAccount)
                .FirstOrDefaultAsync(cp => cp.UserAccountId == userAccount.Id);

            var appointments = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Provider)
                    .ThenInclude(vp => vp.UserAccount)
                .Where(a => a.ClientUserAccountId == userAccount.Id
                         && a.AppointmentDate >= DateTime.Now)
                .OrderBy(a => a.AppointmentDate)
                .Take(5)
                .ToListAsync();

            var recentActivities = await _context.ActivityLogs
                .Where(al => al.UserAccountId == userAccount.Id)
                .OrderByDescending(al => al.Timestamp)
                .Take(6)
                .ToListAsync();

            // ── 3. Map to ViewModel (never pass raw EF models to the view) ───────
            var vm = new ClientDashboard
            {
                FirstName = clientProfile?.UserAccount?.FirstName ?? userAccount.UserName ?? "Friend",
                LastName = clientProfile?.UserAccount?.LastName ?? string.Empty,
                AvatarUrl = clientProfile?.AvatarUrl,
                MemberSince = userAccount.CreatedAt.ToString("MMMM yyyy"),
                IsPremium = clientProfile?.IsPremiumMember ?? false,

                // ── Quick stats ───────────────────────────────────────────────────
                QuickStats = new List<QuickStatViewModel>
                {
                    new() { Value = (clientProfile?.Pets?.Count ?? 0).ToString(),
                            Label = "My Pets",      Icon = "bi-heart-fill",      ColorClass = "stat-coral"   },
                    new() { Value = appointments.Count.ToString(),
                            Label = "Upcoming",     Icon = "bi-calendar2-check", ColorClass = "stat-tan"     },
                    new() { Value = clientProfile?.TotalVisits.ToString() ?? "0",
                            Label = "Total Visits", Icon = "bi-clipboard2-pulse",ColorClass = "stat-brown"   },
                    new() { Value = clientProfile?.LoyaltyPoints.ToString() ?? "0",
                            Label = "Paw Points",   Icon = "bi-stars",           ColorClass = "stat-gold"    },
                },

                // ── Pets ──────────────────────────────────────────────────────────
                Pets = (clientProfile?.Pets ?? new List<Pet>()).Select(p => new PetSummaryViewModel
                {
                    PetId = p.PetId,
                    Name = p.Name,
                    Species = p.Species,
                    Breed = p.Breed,
                    AgeYears = p.AgeYears,
                    PhotoUrl = p.PhotoUrl,
                    HealthBadge = p.HealthStatus,
                    IsVerified = p.IsVerified,
                    VerifiedByVetName = p.VerifiedByVet?.UserAccount?.FullName ?? string.Empty,
                }).ToList(),

                // ── Appointments ──────────────────────────────────────────────────
                UpcomingAppointments = appointments.Select(a => new UpcomingAppointmentViewModel
                {
                    AppointmentId = a.AppointmentId,
                    PetName = a.Pet?.Name ?? "—",
                    ServiceType = a.ServiceType,
                    ProviderName = a.Provider?.UserAccount?.FullName ?? "—",
                    AppointmentDate = a.AppointmentDate,
                    StatusBadge = a.Status,
                    LocationName = a.LocationName ?? string.Empty,
                }).ToList(),

                // ── Activity feed ─────────────────────────────────────────────────
                RecentActivities = recentActivities.Select(al => new RecentActivityViewModel
                {
                    Icon = al.IconClass,
                    Title = al.Title,
                    Description = al.Description,
                    Timestamp = al.Timestamp,
                }).ToList(),
            };

            return View(vm);
        }


        // ════════════════════════════════════════════════════════════════════════
        // MISSING ROUTING ENDPOINTS (The "Fix")
        // ════════════════════════════════════════════════════════════════════════

        // GET: /Client/AddPet
        [HttpGet]
        public IActionResult AddPet()
        {
            return View();
        }

        // ════════════════════════════════════════════════════════════════════════
        // ADD PET (Form Submission)
        // ════════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPet(string petName, string species, string breed, int age)
        {
            var userAccount = await _userManager.GetUserAsync(User);
            if (userAccount == null) return RedirectToAction("Login", "Account");

            var clientProfile = await _context.ClientProfiles.FirstOrDefaultAsync(cp => cp.UserAccountId == userAccount.Id);

            // THE FIX: Auto-create the profile here too so the save doesn't fail
            if (clientProfile == null)
            {
                clientProfile = new ClientProfile { UserAccountId = userAccount.Id };
                _context.ClientProfiles.Add(clientProfile);
                await _context.SaveChangesAsync(); // Save immediately so it generates an ID
            }

            var newPet = new Pet
            {
                Name = petName,
                Species = species,
                Breed = breed ?? "",
                Age = age,
                ClientProfileId = clientProfile.Id, // We now guarantee this ID exists
                HealthStatus = "Healthy",
                PhotoUrl = ""
            };

            _context.Pets.Add(newPet);
            await _context.SaveChangesAsync();

            // Send them to the My Pets page to see their new furry friend
            return RedirectToAction("MyPets");
        }

        // ════════════════════════════════════════════════════════════════════════
        // MY PETS PAGE
        // ════════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> MyPets()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.ClientProfiles
                .Include(cp => cp.Pets)
                .FirstOrDefaultAsync(cp => cp.UserAccountId == user.Id);

            // THE FIX: If the database profile is missing, auto-create it right now!
            if (profile == null)
            {
                profile = new ClientProfile { UserAccountId = user.Id };
                _context.ClientProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            // Safely pass the pets list to the new View you just created
            return View(profile.Pets ?? new List<Pet>());
        }



        // GET: /Client/PetDetail/5
        public IActionResult PetDetail(int id)
        {
            return View();
        }
        // ════════════════════════════════════════════════════════════════════════
        // VIEW APPOINTMENTS PAGE
        // ════════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Appointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.ClientProfiles.FirstOrDefaultAsync(cp => cp.UserAccountId == user.Id);

            // Auto-create profile safety check
            if (profile == null)
            {
                profile = new ClientProfile { UserAccountId = user.Id };
                _context.ClientProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            // Fetch appointments with all the related data (Vet name, Pet name)
            var appointments = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Provider).ThenInclude(v => v.UserAccount)
                .Where(a => a.ClientProfileId == profile.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // ════════════════════════════════════════════════════════════════════════
        // BOOK APPOINTMENT (GET & POST)
        // ════════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> BookAppointment(int vetId)
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.ClientProfiles.Include(p => p.Pets).FirstOrDefaultAsync(c => c.UserAccountId == user.Id);

            if (profile == null || !profile.Pets.Any())
            {
                TempData["ErrorMessage"] = "You must add a pet to your profile before booking an appointment.";
                return RedirectToAction("AddPet");
            }

            // ADDED: Include the Schedule so we know their working hours!
            var vet = await _context.VetProfiles
                .Include(v => v.UserAccount)
                .Include(v => v.Schedule)
                .FirstOrDefaultAsync(v => v.Id == vetId);

            if (vet == null) return NotFound("Vet not found");

            ViewBag.Pets = profile.Pets.ToList();
            ViewBag.Services = await _context.Services.ToListAsync();
            ViewBag.VetName = vet.UserAccount?.FullName ?? vet.ClinicName;
            ViewBag.VetId = vet.Id;

            // FORMAT THE SCHEDULE FOR JAVASCRIPT
            var scheduleList = vet.Schedule?.Select(s => new {
                day = (int)s.Day, // Sunday = 0, Monday = 1, etc.
                isOff = s.IsOff,
                open = s.OpenTime?.ToString(@"hh\:mm") ?? "09:00",
                close = s.CloseTime?.ToString(@"hh\:mm") ?? "17:00"
            }).ToList();

            // Pass the schedule as a JSON string to the view
            ViewBag.VetSchedule = System.Text.Json.JsonSerializer.Serialize(scheduleList);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(int vetId, int petId, int serviceId, DateTime appointmentDate)
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.ClientProfiles.FirstOrDefaultAsync(c => c.UserAccountId == user.Id);

            // Fetch the service so we can save its name
            var service = await _context.Services.FindAsync(serviceId);

            var newAppointment = new Appointment
            {
                ClientProfileId = profile.Id,
                ClientUserAccountId = user.Id,
                PetId = petId,
                ServiceId = serviceId,
                VetProfileId = vetId,
                ServiceType = service?.Name ?? "General Care", 
                AppointmentDate = appointmentDate,
                Status = "Pending"
            };

            _context.Appointments.Add(newAppointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment requested successfully! Waiting for vet confirmation.";
            return RedirectToAction("Appointments");
        }

        // GET: /Client/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Fetch the currently logged-in user's details
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Send the user data directly to the new View
            return View(user);
        }
        // GET: /Client/FindVet
        [HttpGet]
        public IActionResult FindVet()
        {
            // Reroute them to the public directory inside the VetController
            return RedirectToAction("Index", "Vet");
        }

        // ════════════════════════════════════════════════════════════════════════
        // MEDICAL RECORDS PAGE (Client View)
        // ════════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> MedicalRecords()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Fetch the profile and their pets. 
            // Note: Once you build the actual MedicalRecords database table, 
            // you will add `.ThenInclude(p => p.MedicalRecords)` right below the Include line!
            var profile = await _context.ClientProfiles
                .Include(cp => cp.Pets)
                .FirstOrDefaultAsync(cp => cp.UserAccountId == user.Id);

            if (profile == null)
            {
                profile = new ClientProfile { UserAccountId = user.Id };
                _context.ClientProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            // Pass the list of the user's pets to the view
            return View(profile.Pets ?? new List<Pet>());
        }
    }
}