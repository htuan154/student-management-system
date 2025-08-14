    namespace StudentManagementSystem.DTOs.Teacher
    {
        public class TeacherResponseDto
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

            // Computed fields for additional info
            public int UserCount { get; set; }
            public int CourseCount { get; set; }
            public int EnrollmentCount { get; set; }
            public bool IsActive { get; set; } = true;
        }
    }
