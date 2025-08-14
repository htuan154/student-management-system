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

        // Navigation data
        public List<UserInfoDto> Users { get; set; } = new List<UserInfoDto>();
        public List<CourseInfoDto> Courses { get; set; } = new List<CourseInfoDto>();
        public List<EnrollmentInfoDto> Enrollments { get; set; } = new List<EnrollmentInfoDto>();

        // Computed statistics
        public TeacherStatsDto Statistics { get; set; } = new TeacherStatsDto();
    }

    public class UserInfoDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CourseInfoDto
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? Description { get; set; }
        public int EnrolledStudents { get; set; }
    }

    public class EnrollmentInfoDto
    {
        public int EnrollmentId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? FinalGrade { get; set; }
    }

    public class TeacherStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public int PendingEnrollments { get; set; }
        public decimal AverageGrade { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
