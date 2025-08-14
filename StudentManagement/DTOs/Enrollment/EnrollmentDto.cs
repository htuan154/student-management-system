using StudentManagementSystem.Models;
namespace StudentManagementSystem.DTOs.Enrollment
{
    public class EnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string? StudentName { get; set; }
        public string CourseId { get; set; } = string.Empty;
        public string? CourseName { get; set; }
        public int? TeacherCourseId { get; set; }
        public string? TeacherId { get; set; }
        public string? TeacherName { get; set; }

        public int? SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public string? AcademicYear { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
