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
        public string Services { get; set; } = string.Empty;

        public virtual ICollection<WorkingDay> Schedule { get; set; } = new List<WorkingDay>();
    }
}
