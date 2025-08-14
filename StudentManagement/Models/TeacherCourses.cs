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


        [Column(TypeName = "bit")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [Required]
        public int SemesterId { get; set; }

        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; } = null!;

        // Thêm các navigation properties này
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
