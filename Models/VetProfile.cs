using System.ComponentModel.DataAnnotations;

namespace Pets_friends.Models
{
    public class VetProfile
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        public string Title { get; set; } = "Doctor of Veterinary Medicine";
        public string PhotoUrl { get; set; } = "/images/vets/default.jpg";
        public string Specialization { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string ClinicAddress { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Bio { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Phone]
        public string Phone { get; set; } = string.Empty;

        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int HappyPatients { get; set; }

        public List<string> Services { get; set; } = new();
        public List<EducationEntry> Education { get; set; } = new();
        public List<string> Certifications { get; set; } = new();
        public List<WorkingHoursEntry> WorkingHours { get; set; } = new();
        public List<ReviewEntry> Reviews { get; set; } = new();
    }

    public class EducationEntry
    {
        public string Degree { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public int Year { get; set; }
    }

    public class WorkingHoursEntry
    {
        public string Day { get; set; } = string.Empty;
        public string Hours { get; set; } = string.Empty;
        public bool IsOff { get; set; }
    }

    public class ReviewEntry
    {
        public string AuthorName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string PetType { get; set; } = string.Empty;
    }
}