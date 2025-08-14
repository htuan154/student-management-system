namespace StudentManagementSystem.DTOs.Announcement
{
    public class AnnouncementDto
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }
}
