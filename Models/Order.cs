using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public int ClientProfileId { get; set; }
        [ForeignKey("ClientProfileId")]
        public virtual ClientProfile ClientProfile { get; set; } = null!;

        public DateTime OrderDate { get; set; }

        [Precision(18, 2)]
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
