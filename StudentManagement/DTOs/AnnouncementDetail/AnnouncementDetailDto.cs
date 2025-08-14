namespace StudentManagementSystem.DTOs.AnnouncementDetail
{
    public class AnnouncementDetailDto
    {
        public int AnnouncementDetailId { get; set; }
        public int AnnouncementId { get; set; }
        public string? RoleId { get; set; }
        public string? ClassId { get; set; }
        public string? CourseId { get; set; }
        public string? UserId { get; set; }

        // Navigation properties
        public string? RoleName { get; set; }
        public string? ClassName { get; set; }
        public string? CourseName { get; set; }
        public string? UserName { get; set; }
    }
}
