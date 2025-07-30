using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Score
{
    public class ScoreCreateDto
    {
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
