using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pets_friends.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserAccountId { get; set; } = string.Empty;
        [ForeignKey("UserAccountId")]
        public virtual UserAccount UserAccount { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = "bi-activity";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
