using Pets_friends.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class WorkingDay
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DayOfWeek Day { get; set; }

        public TimeSpan? OpenTime { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan? CloseTime { get; set; } = new TimeSpan(17, 0, 0);
        public bool IsOff { get; set; } = false;

        // Nullable FKs!
        public int? VetProfileId { get; set; }
        [ForeignKey("VetProfileId")]
        public virtual VetProfile? VetProfile { get; set; }

        public int? ShelterProfileId { get; set; }
        [ForeignKey("ShelterProfileId")]
        public virtual ShelterProfile? ShelterProfile { get; set; }
    }
}