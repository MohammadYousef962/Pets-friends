using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Data.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }
}