namespace StudentManagementSystem.DTOs.Enrollment
{
    public class EnrollmentWithScoreDto
    {
        public int EnrollmentId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public int Year { get; set; }
        public string? Status { get; set; }

        // Thông tin điểm
        public decimal? ProcessScore { get; set; }
        public decimal? MidtermScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? TotalScore { get; set; }
        public bool IsPassed { get; set; }
        public string Grade { get; set; } = "N/A";

        // Thêm thông tin hữu ích cho frontend
        public DateTime? EnrollmentDate { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
