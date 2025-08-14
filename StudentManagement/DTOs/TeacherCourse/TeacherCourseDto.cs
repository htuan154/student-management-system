namespace StudentManagementSystem.DTOs.TeacherCourse
{
    public class TeacherCourseDto
    {
        public int TeacherCourseId { get; set; }
        public string TeacherId { get; set; } = string.Empty;
        public string? TeacherName { get; set; }
        public string CourseId { get; set; } = string.Empty;
        public string? CourseName { get; set; }
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public string? AcademicYear { get; set; }
        public bool IsActive { get; set; }
    }
}
