using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pets_friends.Models;

namespace Pets_friends.Data.ViewModels
{
    public class ClientBookingVM
    {
        public int VetProfileId { get; set; }
        public VetProfile? VetProfile { get; set; }

        // Client data payload access properties
        public List<Pet>? ClientPets { get; set; }

        // Form submission parameters populated securely by client multiselect engines
        [Required(ErrorMessage = "Please pick at least one pet to book this visit.")]
        public string SelectedPetIds { get; set; } = string.Empty;

        public string SelectedServices { get; set; } = string.Empty;

        [Required(ErrorMessage = "Target Date selection is mandatory.")]
        public DateTime AppointmentDate { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Target Time selection is mandatory.")]
        public string PreferredTime { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}