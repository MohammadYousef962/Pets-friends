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
    public class ClientBoardingDto
    {
        public int Id { get; set; }
        public string PetName { get; set; }
        public string PetImageUrl { get; set; }
        public string ShelterName { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? PickUpDate { get; set; }
        public string Status { get; set; }
    }

    public class ClientAdoptionDto
    {
        public int Id { get; set; }
        public string PetName { get; set; }
        public string PetImageUrl { get; set; }
        public string Type { get; set; }
        public string ShelterName { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; }
    }
    public class ClientAppointmentsVM
    {
        public List<AppointmentDisplayVM> UpcomingAppointments { get; set; } = new List<AppointmentDisplayVM>();
        public List<AppointmentDisplayVM> PastAppointments { get; set; } = new List<AppointmentDisplayVM>();
        public List<ClientBoardingDto> BoardingSessions { get; set; } = new List<ClientBoardingDto>();
        public List<ClientAdoptionDto> AdoptionApplications { get; set; } = new List<ClientAdoptionDto>();
    }
}