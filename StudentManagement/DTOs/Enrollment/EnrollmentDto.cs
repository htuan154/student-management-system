using StudentManagementSystem.Models;
namespace StudentManagementSystem.DTOs.Enrollment
{
    public class EnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string? TeacherId { get; set; }

        public string? Semester { get; set; }
        public int? Year { get; set; }
        public string Status { get; set; } = "Enrolled";
    }
}
