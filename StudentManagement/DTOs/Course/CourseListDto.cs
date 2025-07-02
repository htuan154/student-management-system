namespace StudentManagementSystem.Dtos.Course
{
    public class CourseListDto
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? Department { get; set; }
        public bool IsActive { get; set; }
    }
}
