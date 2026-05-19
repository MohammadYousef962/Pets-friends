using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class ShelterDisplayVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class SheltersDirectoryVM
    {
        public List<ShelterDisplayVM> Shelters { get; set; } = new List<ShelterDisplayVM>();
        public string CurrentQuery { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
}