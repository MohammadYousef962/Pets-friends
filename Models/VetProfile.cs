using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class VetProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserAccountId { get; set; } = string.Empty;

        [ForeignKey("UserAccountId")]
        public virtual UserAccount UserAccount { get; set; } = null!;

        public string Specialization { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string ClinicAddress { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = "https://placehold.co/160x160/C8A882/white?text=Vet";
        public string Services { get; set; } = string.Empty;

        public virtual ICollection<VetReview> Reviews { get; set; } = new List<VetReview>();
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
        public int TotalReviews => Reviews.Count;

        public virtual ICollection<WorkingDay> Schedule { get; set; } = new List<WorkingDay>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    }
}