using System;
using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Data.ViewModels
{
    public class AdoptionApplicationVM
    {
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please tell us why you want this pet.")] // Now Required works!
        public string Motivation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please describe your living situation.")]
        public string LivingSituation { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the check-in policy.")]
        public bool AgreedToPolicy { get; set; }
    }
}