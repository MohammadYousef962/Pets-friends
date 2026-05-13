using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Data.ViewModels
{
    public class MerchantProfileFormVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Your full name is required.")]
        [StringLength(20)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Store Name is required.")]
        [StringLength(100)]
        public string StoreName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Store Address is required.")]
        [StringLength(250)]
        public string StoreAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact Email is required.")]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        public string PhoneNumber { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageUrl { get; set; }
    }
}