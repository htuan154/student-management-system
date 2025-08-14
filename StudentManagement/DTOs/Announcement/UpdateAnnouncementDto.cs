using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Announcement
{
    public class UpdateAnnouncementDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime? ExpiryDate { get; set; }

        // Thông tin targeting (có thể update)
        public List<string>? RoleIds { get; set; }
        public List<string>? ClassIds { get; set; }
        public List<string>? CourseIds { get; set; }
        public List<string>? UserIds { get; set; }
    }
}
