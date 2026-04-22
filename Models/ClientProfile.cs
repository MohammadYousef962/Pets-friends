using Pets_friends.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Pets_friends.Models
{
    public class ClientProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserAccountId { get; set; } = string.Empty;
        [ForeignKey("UserAccountId")]
        public virtual UserAccount UserAccount { get; set; } = null!;

        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}
