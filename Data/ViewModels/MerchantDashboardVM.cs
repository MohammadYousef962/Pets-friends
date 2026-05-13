using Pets_friends.Models;
using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class MerchantDashboardVM
    {
        // 1. Basic Store Info
        public MerchantProfile Profile { get; set; } = null!;

        // 2. KPI Stats
        public decimal TotalRevenue { get; set; }
        public int ActiveOrdersCount { get; set; }
        public int TotalProductsCount { get; set; }

        // (We removed Store Rating from the UI, but it's fine to keep this here if you use it later!)
        public double AverageRating { get; set; }

        // IMPORTANT: Uncommented this so the Low Stock Alerts on the dashboard work!
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Product> StoreProducts { get; set; }
    }
}