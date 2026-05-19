using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class ClientProfile
    {
        [Key]
        public int Id { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        // --- LINK TO PRIMARY IDENTITY ACCOUNT ---
        public string UserAccountId { get; set; } = string.Empty;
        [ForeignKey("UserAccountId")]

        public string? ImageUrl { get; set; }
        public virtual UserAccount UserAccount { get; set; } = null!;

        // --- ONE-TO-MANY: A client can own multiple pets ---
        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}