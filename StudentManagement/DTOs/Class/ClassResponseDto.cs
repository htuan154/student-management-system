namespace StudentManagementSystem.DTOs.Class
{
    public class ClassResponseDto
    {
        public string ClassId { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public string? AcademicYear { get; set; }
        public bool IsActive { get; set; }
        public int StudentCount { get; set; }
    }
}
