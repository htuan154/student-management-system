using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        [StringLength(10)]
        public string CourseId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Credits must be greater than 0")]
        public int Credits { get; set; }

        [StringLength(50)]
        public string? Department { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "bit")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<TeacherCourse> TeacherCourses { get; set; } = new List<TeacherCourse>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
