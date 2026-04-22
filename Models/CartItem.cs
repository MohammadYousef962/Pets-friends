using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public int ClientProfileId { get; set; }
        [ForeignKey("ClientProfileId")]
        public virtual ClientProfile ClientProfile { get; set; } = null!;

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        public int Quantity { get; set; }
    }
}
