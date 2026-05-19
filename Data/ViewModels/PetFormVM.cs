using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Data.ViewModels
{
    public class PetFormVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Pet name is required.")]
        public string Name { get; set; }

        [Required]
        public string Breed { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = DateTime.Now;

        [Required]
        public string Gender { get; set; }

        public bool IsNeutered { get; set; }

        public string Description { get; set; }

        public string? MedicalHistory { get; set; }

        // VET ONLY: Holds the ID of the client they select from the dropdown
        public int? SelectedClientProfileId { get; set; }

        // SHELTER ONLY: Holds the toggle state for the public adoption board
        public bool IsPubliclyListed { get; set; } = true;

        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageUrl { get; set; }
    }
}