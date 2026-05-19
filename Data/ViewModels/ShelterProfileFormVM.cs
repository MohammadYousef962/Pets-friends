using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Data.ViewModels
{
    public class ShelterProfileFormVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Manager's Full Name is required.")]
        [StringLength(30, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Shelter Name is required.")]
        [StringLength(50, ErrorMessage = "Shelter Name cannot exceed 100 characters.")]
        public string ShelterName { get; set; }

        [Required(ErrorMessage = "Shelter Address is required.")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Contact Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(60)]
        public string Email { get; set; }

        [Required(ErrorMessage = "A description of the shelter is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        // The '?' stops the hidden validation crashes!
        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageUrl { get; set; }

        public List<WorkingDayVM> Schedule { get; set; } = new List<WorkingDayVM>();
    }
}