namespace StudentManagementSystem.Dtos.TeacherCourse
{
    public class TeacherCourseDto
    {
        public int TeacherCourseId { get; set; }
        public string TeacherId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string? Semester { get; set; }
        public int? Year { get; set; }
        public bool IsActive { get; set; }
    }
}