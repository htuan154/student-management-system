using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("Scores")]
    public class Score
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScoreId { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        [Range(0, 10, ErrorMessage = "Process score must be between 0 and 10")]
        public decimal? ProcessScore { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        [Range(0, 10, ErrorMessage = "Midterm score must be between 0 and 10")]
        public decimal? MidtermScore { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        [Range(0, 10, ErrorMessage = "Final score must be between 0 and 10")]
        public decimal? FinalScore { get; set; }

        // Computed properties (calculated fields)
        [Column(TypeName = "decimal(4,2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? TotalScore { get; private set; }

        [Column(TypeName = "bit")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool? IsPassed { get; private set; }

        // Navigation properties
        [ForeignKey("EnrollmentId")]
        public virtual Enrollment Enrollment { get; set; } = null!;

        // Helper method to calculate total score manually if needed
        public decimal? CalculateTotalScore()
        {
            if (ProcessScore.HasValue && MidtermScore.HasValue && FinalScore.HasValue)
            {
                return Math.Round((decimal)(ProcessScore * 0.2m + MidtermScore * 0.3m + FinalScore * 0.5m), 2);
            }
            return null;
        }

        // Helper method to check if passed manually if needed
        public bool? CalculateIsPassed()
        {
            var total = CalculateTotalScore();
            if (total.HasValue)
            {
                return total >= 4.0m;
            }
            return null;
        }
    }
}
