using Pets_friends.Models;
using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels
{
    public class ProductDetailsVM
    {
        public Product Product { get; set; } = null!;

        // CHANGED: Correctly uses your ProductReview entity
        public List<ProductReview> Reviews { get; set; } = new List<ProductReview>();

        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public int SelectedQuantity { get; set; } = 1;
    }
}