namespace StudentManagementSystem.Dtos.Score
{
    public class ScoreDetailsDto
    {
        public int ScoreId { get; set; }
        public decimal? ProcessScore { get; set; }
        public decimal? MidtermScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? TotalScore { get; set; }
        public bool? IsPassed { get; set; }

        // Thông tin sinh viên
        public string? StudentId { get; set; }
        public string? FullName { get; set; }
    }
}
