using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty; // "Toy", "Food", "Medicine"

        [Precision(18, 2)] // Required for Money
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int MerchantProfileId { get; set; }
        [ForeignKey("MerchantProfileId")]
        public virtual MerchantProfile MerchantProfile { get; set; } = null!;
        public string ImageUrl { get; set; } = "https://placehold.co/300x300/FAF6F1/8C7560?text=No+Image";
        public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}