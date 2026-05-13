using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // --- Link to the Buyer ---
        public int ClientProfileId { get; set; }
        [ForeignKey("ClientProfileId")]
        public virtual ClientProfile ClientProfile { get; set; } = null!;

        // ---> CRITICAL: Link to the Store (Merchant) <---
        public int MerchantProfileId { get; set; }
        [ForeignKey("MerchantProfileId")]
        public virtual MerchantProfile MerchantProfile { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered, Cancelled

        public decimal TotalAmount { get; set; }

        // --- The Purchased Items ---
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}