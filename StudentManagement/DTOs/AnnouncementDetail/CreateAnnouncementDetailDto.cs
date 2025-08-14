using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.AnnouncementDetail
{
    public class CreateAnnouncementDetailDto
    {
        [Required]
        public int AnnouncementId { get; set; }

        public string? RoleId { get; set; }
        public string? ClassId { get; set; }
        public string? CourseId { get; set; }
        public string? UserId { get; set; }
    }
}
