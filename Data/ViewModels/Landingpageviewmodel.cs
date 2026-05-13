using Pets_friends.Models;
using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class ServiceCardViewModel
    {
        public int Id { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsFeatured { get; set; } = false;

    }

    public class LandingPageViewModel
    {
        public List<Service> FeaturedServices { get; set; } = new List<Service>();
        public List<ServiceCardViewModel> Services { get; set; } = new List<ServiceCardViewModel>
    {
        new ServiceCardViewModel { Id = 1, Title = "Veterinary Care", Description = "Our experienced vets provide top-notch medical care to keep your furry friend healthy.", Icon = "bi-heart-pulse-fill" },
        new ServiceCardViewModel { Id = 2, Title = "Pet Boarding", Description = "Leave your pet with us while you're away, knowing they'll receive love and attention.", Icon = "bi-house-heart-fill" },
        new ServiceCardViewModel { Id = 3, Title = "Dog Walking", Description = "Our trained walkers ensure your dog gets the exercise they need to stay happy.", Icon = "bi-person-walking" },
        new ServiceCardViewModel { Id = 4, Title = "Grooming Services", Description = "Keep your pet looking their best with our professional grooming services.", Icon = "bi-scissors" },
        new ServiceCardViewModel { Id = 5, Title = "Pet Training", Description = "Our expert trainers help your pet learn good behavior and obedience.", Icon = "bi-journal-check" },
        new ServiceCardViewModel { Id = 6, Title = "Pet Sitting", Description = "Our reliable pet sitters provide care and companionship for your pet at home.", Icon = "bi-person-bounding-box" },
        new ServiceCardViewModel { Id = 7, Title = "Pet Adoption", Description = "Find your new best friend among our adorable pets waiting for a loving home.", Icon = "bi-heart-fill" },
        new ServiceCardViewModel { Id = 8, Title = "Pet Supplies", Description = "Shop our wide selection of pet supplies to keep your furry friend happy and healthy.", Icon = "bi-bag-heart-fill" },
        new ServiceCardViewModel { Id = 9, Title = "Pet Transportation", Description = "Our safe and comfortable transportation services ensure your pet gets where they need to go.", Icon = "bi-truck" },
        new ServiceCardViewModel { Id = 10, Title = "Pet Photography", Description = "Capture precious moments with our professional pet photography services.", Icon = "bi-camera-fill" },
        new ServiceCardViewModel { Id = 11, Title = "Pet Daycare", Description = "Our fun and safe daycare provides socialization and care for your pet during the day.", Icon = "bi-people-fill" }
    };
    }
}