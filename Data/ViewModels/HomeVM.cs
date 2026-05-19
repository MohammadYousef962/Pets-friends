using System;
using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class HomeVM
    {
        public List<HomePetDto> AdoptablePets { get; set; } = new List<HomePetDto>();
        public List<HomeVetDto> TopClinics { get; set; } = new List<HomeVetDto>();
        public List<HomeProductDto> FeaturedProducts { get; set; } = new List<HomeProductDto>();

        // Overall Vet Statistics
        public double OverallVetAverageRating { get; set; }
        public int TotalVetReviews { get; set; }

        // The 2 most recent reviews
        public List<TestimonialDto> RecentTestimonials { get; set; } = new List<TestimonialDto>();
    }

    public class TestimonialDto
    {
        public string Comment { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int Rating { get; set; }
    }

    public class HomePetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ShelterName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public bool IsNeutered { get; set; }
        public string MedicalHistory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class HomeVetDto
    {
        public int Id { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public string Specialties { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string RecentReview { get; set; } = string.Empty;
    }

    public class HomeProductDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
    }
}