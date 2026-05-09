using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class MerchantProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserAccountId { get; set; } = string.Empty;
        [ForeignKey("UserAccountId")]
        public virtual UserAccount UserAccount { get; set; } = null!;

        public string StoreName { get; set; } = string.Empty;

        // --- ADDED THIS LINE ---
        public string StoreAddress { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = "https://placehold.co/160x160/C8A882/white?text=merchant";

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}