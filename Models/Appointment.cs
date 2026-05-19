// Models/Appointment.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        public int ClientProfileId { get; set; }
        [ForeignKey("ClientProfileId")]
        public virtual ClientProfile ClientProfile { get; set; } = null!;

        // ── MAKE THESE TWO LINES NULLABLE (?) ──
        public int? PetId { get; set; }
        [ForeignKey("PetId")]
        public virtual Pet? Pet { get; set; }

        public int? ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        public int VetProfileId { get; set; }
        [ForeignKey("VetProfileId")]
        public virtual VetProfile VetProfile { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "Pending";

        public string? Notes { get; set; }
        public bool IsUrgent { get; set; }
    }
}