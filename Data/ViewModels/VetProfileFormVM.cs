using Microsoft.AspNetCore.Http; // Required for file upload
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Data.ViewModels
{
    public class VetProfileFormVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Specialization is required.")]
        public string Specialization { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinic Name is required.")]
        public string ClinicName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinic Address is required.")]
        public string ClinicAddress { get; set; } = string.Empty;

        [Required]
        [Range(0, 60, ErrorMessage = "Experience must be between 0 and 60 years.")]
        public int YearsOfExperience { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        // --- PHYSICAL FILE UPLOAD ---
        public IFormFile? ImageFile { get; set; }

        // --- DISPLAY CURRENT IMAGE ---
        public string? ExistingImageUrl { get; set; }

        public string Services { get; set; } = string.Empty;

        // --- SCHEDULES ---
        public List<WorkingDayVM> Schedule { get; set; } = new List<WorkingDayVM>();
    }
}