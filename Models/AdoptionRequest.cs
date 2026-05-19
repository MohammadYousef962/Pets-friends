using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class AdoptionRequest
    {
        [Key]
        public int Id { get; set; }
        public int PetId { get; set; }
        public int ShelterProfileId { get; set; }
        public int ClientProfileId { get; set; }
        public string ApplicantId { get; set; } = string.Empty;
        public string Motivation { get; set; } = string.Empty;
        public string LivingSituation { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime RequestDate { get; set; }
    }
}