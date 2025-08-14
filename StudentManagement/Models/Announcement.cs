using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("Announcements")]
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ExpiryDate { get; set; }

        // Navigation properties
        [ForeignKey("CreatedBy")]
        public virtual User User { get; set; } = null!;
        public virtual ICollection<AnnouncementDetail> AnnouncementDetails { get; set; } = new List<AnnouncementDetail>();
    }
}
