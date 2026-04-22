using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class VetReview
    {
        [Key]
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required, MaxLength(500)]
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int VetProfileId { get; set; }
        [ForeignKey("VetProfileId")]
        public virtual VetProfile VetProfile { get; set; } = null!;

        public string ReviewerId { get; set; } = string.Empty;
        [ForeignKey("ReviewerId")]
        public virtual UserAccount Reviewer { get; set; } = null!;
    }
}