namespace StudentManagementSystem.DTOs.AnnouncementDetail
{
    public class BulkCreateAnnouncementDetailDto
    {
        public List<string>? RoleIds { get; set; }
        public List<string>? ClassIds { get; set; }
        public List<string>? CourseIds { get; set; }
        public List<string>? UserIds { get; set; }
    }
}
