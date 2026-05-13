using System;
using System.Collections.Generic;

namespace PetFriends.ViewModels.Client
{
    // ─────────────────────────────────────────────
    //  Supporting nested view-models
    // ─────────────────────────────────────────────

    public class PetSummaryViewModel
    {
        public int PetId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;   // e.g. "Dog", "Cat"
        public string Breed { get; set; } = string.Empty;
        public int AgeYears { get; set; }
        public string? PhotoUrl { get; set; }                    // nullable – placeholder shown if null
        public string HealthBadge { get; set; } = "Healthy";      // "Healthy" | "Needs Attention" | "Critical"
        public bool IsVerified { get; set; } = false;            // Shows green checkmark if verified by vet
        public string? VerifiedByVetName { get; set; }            // Name of vet who verified
    }

    public class UpcomingAppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;   // "Vet Visit", "Grooming", etc.
        public string ProviderName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string StatusBadge { get; set; } = "Confirmed";    // "Confirmed" | "Pending" | "Cancelled"
        public string LocationName { get; set; } = string.Empty;
    }

    public class RecentActivityViewModel
    {
        public string Icon { get; set; } = "bi-activity";  // Bootstrap Icons class
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class QuickStatViewModel
    {
        public string Value { get; set; } = string.Empty;   // e.g. "3", "99%"
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;   // Bootstrap Icons class
        public string ColorClass { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────
    //  Root Dashboard ViewModel
    // ─────────────────────────────────────────────

    public class ClientDashboard
    {
        // ── Profile section ──────────────────────
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string MemberSince { get; set; } = string.Empty;   // formatted, e.g. "March 2022"
        public bool IsPremium { get; set; }

        // Convenience
        public string FullName => $"{FirstName} {LastName}";

        // ── Quick stats bar ──────────────────────
        public List<QuickStatViewModel> QuickStats { get; set; } = new();

        // ── Pets ─────────────────────────────────
        public List<PetSummaryViewModel> Pets { get; set; } = new();

        // ── Appointments ─────────────────────────
        public List<UpcomingAppointmentViewModel> UpcomingAppointments { get; set; } = new();

        // ── Activity feed ────────────────────────
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();

        // ── Greeting logic ───────────────────────
        public string Greeting
        {
            get
            {
                int hour = DateTime.Now.Hour;
                return hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
            }
        }
    }
}
