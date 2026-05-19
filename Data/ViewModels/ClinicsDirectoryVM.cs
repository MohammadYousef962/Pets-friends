using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class ClinicsDirectoryVM
    {
        public List<VetClinicDto> Clinics { get; set; } = new List<VetClinicDto>();
        public string CurrentQuery { get; set; } = string.Empty;
        public string CurrentCategory { get; set; } = "all";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class VetClinicDto
    {
        public int Id { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Specialties { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}