using Pets_friends.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pets_friends.Data.ViewModels
{
    public class CartVM
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // Dynamic calculations
        public decimal Subtotal => Items.Sum(i => i.Product.Price * i.Quantity);
        public decimal Tax => Subtotal * 0.08m; // 8% tax example
        public decimal Total => Subtotal + Tax;
    }

    public class CartItem
    {
        public int Id { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
    }
}