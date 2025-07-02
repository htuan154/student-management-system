using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Dtos.Score
{
    public class ScoreUpdateDto
    {
        [Required]
        public int ScoreId { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [Range(0, 10)]
        public decimal? ProcessScore { get; set; }

        [Range(0, 10)]
        public decimal? MidtermScore { get; set; }

        [Range(0, 10)]
        public decimal? FinalScore { get; set; }
    }
}