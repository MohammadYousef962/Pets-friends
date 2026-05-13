using Pets_friends.Models;
using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class ProductDetailsVM
    {
        public Product Product { get; set; } = null!;

        // Strictly uses your established ProductReview database entity
        public List<ProductReview> Reviews { get; set; } = new List<ProductReview>();

        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        // RESTORED: Prevents the controller build/publish crash
        public int SelectedQuantity { get; set; } = 1;
    }
}