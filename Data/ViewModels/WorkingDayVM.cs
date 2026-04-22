// Data/ViewModels/WorkingDayVM.cs
namespace Pets_friends.Data.ViewModels
{
    // A simplified model specifically for the edit form
    public class WorkingDayVM
    {
        public int Id { get; set; }
        public DayOfWeek Day { get; set; }

        // We use strings for times here for easy model binding in the form
        // (The Controller handles converting these to TimeSpans)
        public string? OpenTime { get; set; }
        public string? CloseTime { get; set; }
        public bool IsOff { get; set; }
    }
}