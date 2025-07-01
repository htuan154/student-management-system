using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("TeacherCourses")]
    public class TeacherCourse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TeacherCourseId { get; set; }

        [Required]
        [StringLength(10)]
        public string TeacherId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CourseId { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Semester { get; set; }

        public int? Year { get; set; }

        [Column(TypeName = "bit")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;
    }
}
