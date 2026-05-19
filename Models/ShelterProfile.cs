using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class ShelterProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserAccountId { get; set; } = string.Empty;

        [ForeignKey("UserAccountId")]
        public virtual UserAccount UserAccount { get; set; } = null!;

        public string ShelterName { get; set; } = string.Empty;
        public string ShelterAddress { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = "https://placehold.co/160x160/C8A882/white?text=shelter";
        public string Address { get; set; } = string.Empty;

        // "Services" is completely gone from here!

        public virtual ICollection<WorkingDay> Schedule { get; set; } = new List<WorkingDay>();
        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}