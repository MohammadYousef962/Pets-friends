using System;
using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class AppointmentDisplayVM
    {
        public int Id { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string PetImageUrl { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string ClinicAddress { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty; // "Pending", "Confirmed", "Completed", "Cancelled"
    }

    public class ClientAppointmentsVM
    {
        public List<AppointmentDisplayVM> UpcomingAppointments { get; set; } = new List<AppointmentDisplayVM>();
        public List<AppointmentDisplayVM> PastAppointments { get; set; } = new List<AppointmentDisplayVM>();
    }
}