using Pets_friends.Models;
using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class VetDashboardVM
    {
        public VetProfile Profile { get; set; }

        // Change this line to use VetReview!
        public IEnumerable<VetReview> RecentReviews { get; set; }

        public IEnumerable<Appointment> PendingAppointments { get; set; }
    }
}