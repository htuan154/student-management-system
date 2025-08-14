using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("AnnouncementDetails")]
    public class AnnouncementDetail
    {
        [Key]
        public int AnnouncementDetailId { get; set; }

        public int AnnouncementId { get; set; }

        [StringLength(10)]
        public string? RoleId { get; set; }

        [StringLength(10)]
        public string? ClassId { get; set; }

        [StringLength(10)]
        public string? CourseId { get; set; }

        [StringLength(10)]
        public string? UserId { get; set; }

        // Navigation properties
        [ForeignKey("AnnouncementId")]
        public virtual Announcement Announcement { get; set; } = null!;
        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }
        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
