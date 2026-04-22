using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Precision(18, 2)] // Required for Money
        public decimal Price { get; set; }

        // Nullable FKs!
        public int? VetProfileId { get; set; }
        [ForeignKey("VetProfileId")]
        public virtual VetProfile? VetProfile { get; set; }

        public int? ShelterProfileId { get; set; }
        [ForeignKey("ShelterProfileId")]
        public virtual ShelterProfile? ShelterProfile { get; set; }
    }
}
