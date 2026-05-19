using Pets_friends.Models;
using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class ShelterProfileVM
    {
        public int Id { get; set; }
        public string UserAccountId { get; set; }
        public string FullName { get; set; }
        public string ShelterName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string ImageUrl { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string Services { get; set; }

        public int TotalAdoptions { get; set; }
        public int AvailablePetsCount { get; set; }

        public List<WorkingDay> Schedule { get; set; } = new List<WorkingDay>();
        public List<ShelterPetDto> AvailablePets { get; set; } = new List<ShelterPetDto>();
    }

    public class ShelterPetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ShelterName { get; set; } = string.Empty; // Added
        public string Gender { get; set; } = string.Empty;     // Added
        public bool IsNeutered { get; set; }                    // Added
        public string MedicalHistory { get; set; } = string.Empty; // Added
        public string Description { get; set; } = string.Empty;    // Added
    }
}