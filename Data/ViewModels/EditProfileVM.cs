using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Data.ViewModels
{
    public class EditProfileVM
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Gender { get; set; }

        public IFormFile? ProfilePhoto { get; set; }

        public string? City { get; set; }
    }
}