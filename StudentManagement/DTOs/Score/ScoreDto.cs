namespace StudentManagementSystem.DTOs.Score
{
    public class ScoreDto
    {
        public int ScoreId { get; set; }
        public int EnrollmentId { get; set; }
        public decimal? ProcessScore { get; set; }
        public decimal? MidtermScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? TotalScore { get; set; }
        public bool? IsPassed { get; set; }

    }
}
