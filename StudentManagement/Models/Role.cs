using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        [StringLength(10)]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<AnnouncementDetail> AnnouncementDetails { get; set; } = new List<AnnouncementDetail>();
    }
}
