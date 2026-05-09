using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Data.ViewModels
{
    public class ProductFormVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.00, 10000, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, 10000, ErrorMessage = "Stock cannot be negative.")]
        public int StockQuantity { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        // For Product Photo Uploads
        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageUrl { get; set; }
    }
}