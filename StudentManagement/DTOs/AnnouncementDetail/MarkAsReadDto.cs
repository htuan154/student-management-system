using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.AnnouncementDetail
{
    public class MarkAsReadDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int AnnouncementId { get; set; }
    }
}