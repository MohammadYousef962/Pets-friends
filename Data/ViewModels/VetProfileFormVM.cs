using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Data.ViewModels
{
    public class VetProfileFormVM
    {
        public int Id { get; set; }

        // --- ADDED: Maps directly to the root registration handle ---
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(25)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinic Name is required.")]
        [StringLength(50)]
        public string ClinicName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinic Address is required.")]
        [StringLength(200)]
        public string ClinicAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Invalid format. Must be name@domain.com (or .jo, .net, etc)")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [RegularExpression(@"^\+?[0-9\s-]{8,20}$",
            ErrorMessage = "Invalid phone format. (8-20 characters required)")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialization is required.")]
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [Range(0, 60, ErrorMessage = "Experience must be between 0 and 60 years.")]
        public int YearsOfExperience { get; set; }

        [Required(ErrorMessage = "Bio is required.")]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageUrl { get; set; }

        [Required(ErrorMessage = "Please add at least one service.")]
        public string Services { get; set; } = string.Empty;

        public List<WorkingDayVM> Schedule { get; set; } = new List<WorkingDayVM>();
    }
}