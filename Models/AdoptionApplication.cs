using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class AdoptionApplication
    {
        [Key]
        public int Id { get; set; }

        public int PetId { get; set; }
        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; }

        public int ClientProfileId { get; set; }
        [ForeignKey("ClientProfileId")]
        public virtual ClientProfile ClientProfile { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string Type { get; set; } = "Adoption"; // Adoption, Transfer

        public DateTime ApplicationDate { get; set; } = DateTime.Now;
    }
}