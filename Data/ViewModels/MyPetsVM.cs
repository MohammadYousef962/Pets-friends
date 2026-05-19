using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Pets_friends.Data.ViewModels
{
    public class MyPetDisplayVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age => CalculateAge(DateOfBirth);
        public string Gender { get; set; } = string.Empty;
        public bool IsNeutered { get; set; }
        public string MedicalHistory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime? LastNameChangeDate { get; set; }

        // Logic to check if 90 days have passed
        public bool CanChangeName => !LastNameChangeDate.HasValue || (DateTime.Now - LastNameChangeDate.Value).TotalDays >= 90;
        public DateTime? NextNameChangeDate => LastNameChangeDate?.AddDays(90);

        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    public class EditClientPetVM
    {
        [Required]
        public int PetId { get; set; }

        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string? ExistingImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}