namespace StudentManagementSystem.DTOs.Teacher
{
    public class TeacherDetailResponseDto
    {
        public string TeacherId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? Degree { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime HireDate { get; set; }
        public decimal? Salary { get; set; }
        public List<CourseInfoDto> Courses { get; set; } = new List<CourseInfoDto>();
        public List<EnrollmentInfoDto> Enrollments { get; set; } = new List<EnrollmentInfoDto>();
    }

    public class CourseInfoDto
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
    }

    public class EnrollmentInfoDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
