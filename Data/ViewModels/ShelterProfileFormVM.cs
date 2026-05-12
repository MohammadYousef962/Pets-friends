using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Data.ViewModels
{
    // This ViewModel is used by the Create and Edit shelter forms.
    // It collects all the information the shelter user enters in the page.
    public class ShelterProfileFormVM
    {
        // Used when editing an existing shelter profile
        public int Id { get; set; }

        // Shelter basic info
        [Required(ErrorMessage = "Shelter Name is required.")]
        [StringLength(100)]
        public string ShelterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shelter Address is required.")]
        [StringLength(200)]
        public string ShelterAddress { get; set; } = string.Empty;

        // Contact email for the shelter account
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Invalid format. Must be name@domain.com")]
        public string Email { get; set; } = string.Empty;

        // Contact phone number
        [Required(ErrorMessage = "Phone Number is required.")]
        [RegularExpression(@"^\+?[0-9\s-]{8,20}$",
            ErrorMessage = "Invalid phone format. (8-20 characters required)")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Description shown on the shelter profile page
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        // Services are stored as one string, separated by commas
        [Required(ErrorMessage = "Please add at least one service.")]
        public string Services { get; set; } = string.Empty;

        // Uploaded image file
        public IFormFile? ImageFile { get; set; }

        // Existing saved image path, useful in Edit page
        public string? ExistingImageUrl { get; set; }

        // Weekly working schedule
        public List<WorkingDayVM> Schedule { get; set; } = new();
    }
}
