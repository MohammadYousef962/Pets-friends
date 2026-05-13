using Microsoft.AspNetCore.Identity;
using Pets_friends.Models;
namespace Pets_friends.Models
{
    public class UserAccount : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? City { get; set; }
        public bool IsProfileComplete { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ClientProfile? ClientProfile { get; set; }
        public virtual VetProfile? VetProfile { get; set; }
        public virtual MerchantProfile? MerchantProfile { get; set; }
        public virtual ShelterProfile? ShelterProfile { get; set; }
    }
}