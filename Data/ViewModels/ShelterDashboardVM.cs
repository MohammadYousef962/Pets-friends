using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class ShelterDashboardVM
    {
        public string ShelterName { get; set; }
        public string ImageUrl { get; set; }

        public int InResidenceCount { get; set; }
        public int PendingAdoptionsCount { get; set; }
        public int IntakeRequestsCount { get; set; }
        public int ActiveBoardingCount { get; set; }

        public List<QueueItemDto> QueueItems { get; set; } = new List<QueueItemDto>();
        public List<BoardingLogDto> BoardingLogs { get; set; } = new List<BoardingLogDto>();
    }

    public class QueueItemDto
    {
        public int Id { get; set; }
        public string Type { get; set; } // Adoption, Transfer
        public string PetName { get; set; }
        public string PetInfo { get; set; }
        public string PetImageUrl { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicantContact { get; set; }
        public string Status { get; set; }
    }

    public class BoardingLogDto
    {
        public int Id { get; set; }
        public string PetName { get; set; }
        public string PetBreed { get; set; }
        public string OwnerName { get; set; }
        public string TimeLabel { get; set; }
        public string StatusType { get; set; }
        public string SpecialNotes { get; set; }
        public DateTime ScheduledDate { get; set; } 
    }
}