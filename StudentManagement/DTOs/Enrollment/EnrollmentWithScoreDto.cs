namespace StudentManagementSystem.DTOs.Enrollment
{
    public class EnrollmentWithScoreDto
    {
        public int EnrollmentId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty; 
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }

        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;

        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        // Score information
        public decimal? ProcessScore { get; set; }
        public decimal? MidtermScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? TotalScore { get; set; }
        public bool IsPassed { get; set; }
        public string Grade { get; set; } = string.Empty;
    }
}
