using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("Semesters")]
    public class Semester
    {
        [Key]
        public int SemesterId { get; set; }

        [Required]
        [StringLength(50)]
        public string SemesterName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "bit")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
        public virtual ICollection<TeacherCourse> TeacherCourses { get; set; } = new List<TeacherCourse>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
