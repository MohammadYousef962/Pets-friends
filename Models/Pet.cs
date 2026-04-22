using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class Pet
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }

        public int ClientProfileId { get; set; }
        [ForeignKey("ClientProfileId")]
        public virtual ClientProfile ClientProfile { get; set; } = null!;
    }
}
