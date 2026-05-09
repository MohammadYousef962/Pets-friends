using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class ProductReview
    {
        [Key]
        public int Id { get; set; }

        // --- Link to the Product ---
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        // --- Link to the Client (Buyer) ---
        public int ClientProfileId { get; set; }
        [ForeignKey("ClientProfileId")]
        public virtual ClientProfile ClientProfile { get; set; } = null!;

        // --- Review Details ---
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please leave a comment.")]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime ReviewDate { get; set; } = DateTime.Now;
    }
}