using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class GlobalPetsVM
    {
        public List<GlobalPetDto> Pets { get; set; } = new List<GlobalPetDto>();

        // Pagination logic
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalPets { get; set; }
    }

        public class GlobalPetDto
        {
            public int PetId { get; set; }
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
}