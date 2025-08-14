using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [StringLength(10)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string RoleId { get; set; } = string.Empty;

        [StringLength(10)]
        public string? StudentId { get; set; }

        [StringLength(10)]
        public string? EmployeeId { get; set; }

        [StringLength(10)]
        public string? TeacherId { get; set; }

        [Column(TypeName = "bit")]
        public bool IsActive { get; set; } = true;

        [NotMapped]
        public string? RefreshToken { get; set; }

        [NotMapped]
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation properties
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher? Teacher { get; set; }

        // Thêm các navigation properties này
        public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
        public virtual ICollection<AnnouncementDetail> AnnouncementDetails { get; set; } = new List<AnnouncementDetail>();
    }
}
