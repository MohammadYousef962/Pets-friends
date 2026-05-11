namespace Pets_friends.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string UserAccountId { get; set; } = null!;
        public int Quantity { get; set; }
    }
}