namespace Pets_friends.Data.ViewModels
{
    public class AdminDashboardVM
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ManageUserVM
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
    }

    // NEW: Unified model for the Transaction History Ledger
    public class TransactionRecordVM
    {
        public int ReferenceId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty; // "Product Order" or "Service Appointment"
        public string CustomerName { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}