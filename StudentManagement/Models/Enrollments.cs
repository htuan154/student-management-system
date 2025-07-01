using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("Enrollments")]
    public class Enrollment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnrollmentId { get; set; }

        [Required]
        [StringLength(10)]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CourseId { get; set; } = string.Empty;

        [StringLength(10)]
        public string? TeacherId { get; set; }

        [StringLength(20)]
        public string? Semester { get; set; }

        public int? Year { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Enrolled";

        // Navigation properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("TeacherId")]
        public virtual Teacher? Teacher { get; set; }

        public virtual Score? Score { get; set; }
    }
}
