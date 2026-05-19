using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class Pet
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? MedicalHistory { get; set; }
        public string? ImageUrl { get; set; }

        public DateTime DateOfBirth { get; set; }

        [NotMapped]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
        public string Gender { get; set; } = string.Empty; // "Male" or "Female"
        public bool IsNeutered { get; set; }

        // Shelter specific: Should this pet appear on the global Adopt page?
        public bool IsPubliclyListed { get; set; }

        // Adopted status
        public bool IsAdopted { get; set; }

        public DateTime? LastNameChangeDate { get; set; }
        // Foreign Keys
        public int? ShelterProfileId { get; set; }
        [ForeignKey("ShelterProfileId")]
        public virtual ShelterProfile? ShelterProfile { get; set; }

        public int? ClientProfileId { get; set; }
        [ForeignKey("ClientProfileId")]
        public virtual ClientProfile? ClientProfile { get; set; }
    }
}