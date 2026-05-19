using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Pets_friends.Models;

namespace Pets_friends.Data.ViewModels
{
    public class ClientDashboardVM
    {
        public string Id { get; set; } = string.Empty;

        // This perfectly formats the ugly database ID into a clean ACC-XXXX format!
        public string FormattedAccountId => !string.IsNullOrEmpty(Id) && Id.Length >= 4
            ? $"ACC-{Id.Substring(0, 4).ToUpper()}"
            : "ACC-0000";

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [StringLength(15, ErrorMessage = "Phone number is too long.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        public string City { get; set; } = string.Empty;

        public string? ExistingImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

        public List<Pet> RecentPets { get; set; } = new List<Pet>();
    }
}