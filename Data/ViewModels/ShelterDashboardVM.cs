using Pets_friends.Models;
using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    // This ViewModel is used by the shelter dashboard page.
    // It collects the profile itself plus some simple calculated values.
    public class ShelterDashboardVM
    {
        // The main shelter profile data
        public ShelterProfile Profile { get; set; } = null!;

        // Number of services listed by the shelter
        public int ServicesCount { get; set; }

        // Number of days the shelter is open
        public int OpenDaysCount { get; set; }

        // A short preview of working days shown on dashboard
        public IEnumerable<WorkingDay> SchedulePreview { get; set; } = new List<WorkingDay>();
    }
}
