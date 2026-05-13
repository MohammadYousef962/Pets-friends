using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }
        public int AppointmentId => Id;  // Alias for compatibility

        public int ClientProfileId { get; set; }
        [ForeignKey("ClientProfileId")]
        public virtual ClientProfile ClientProfile { get; set; } = null!;

        public string? ClientUserAccountId { get; set; }

        public int PetId { get; set; }
        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; } = null!;

        public int ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; } = null!;

        public int VetProfileId { get; set; }
        [ForeignKey("VetProfileId")]
        public virtual VetProfile Provider { get; set; } = null!;  // Vet / Groomer / etc.

        public string ServiceType { get; set; } = string.Empty;
        public string? LocationName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "Pending";
    }
}