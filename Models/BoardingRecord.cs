using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class BoardingRecord
    {
        [Key]
        public int Id { get; set; }

        public int ShelterProfileId { get; set; }
        [ForeignKey("ShelterProfileId")]
        public virtual ShelterProfile ShelterProfile { get; set; }

        // Storing basic info for simplicity, or link to Pet/Client tables
        public string PetName { get; set; }
        public string PetBreed { get; set; }
        public string OwnerName { get; set; }

        public string Status { get; set; } // DropOff, Active, PickUp, Completed
        public string TimeLabel { get; set; }
        public string SpecialNotes { get; set; }
        public DateTime? PickUpDate { get; set; }
        public DateTime ScheduledDate { get; set; }
    }
}