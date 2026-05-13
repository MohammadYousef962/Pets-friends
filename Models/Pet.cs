using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class Pet
    {
        [Key]
        public int Id { get; set; }
        public int PetId => Id;  // Alias for compatibility

        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }
        [NotMapped]
        public int AgeYears => Age;
        public string? PhotoUrl { get; set; }
        public string HealthStatus { get; set; } = "Healthy";

        public int ClientProfileId { get; set; }
        [ForeignKey("ClientProfileId")]
        public virtual ClientProfile ClientProfile { get; set; } = null!;

        // --- NEW: Verification Logic ---

        // Defaults to false when a client adds their own pet
        public bool IsVerified { get; set; } = false;

        // Nullable (?) because an unverified pet doesn't have a verifying vet yet
        public int? VerifiedByVetId { get; set; }
        // create the navigation property.
        [ForeignKey("VerifiedByVetId")]
        public virtual VetProfile? VerifiedByVet { get; set; }
    }
}