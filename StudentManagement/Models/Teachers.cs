using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("Teachers")]
    public class Teacher
    {
        [Key]
        [StringLength(10)]
        public string TeacherId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        public string? Department { get; set; }

        [StringLength(50)]
        public string? Degree { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        [Column(TypeName = "date")]
        public DateTime HireDate { get; set; } = DateTime.Today;

        [Column(TypeName = "decimal(12,2)")]
        public decimal? Salary { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<TeacherCourse> TeacherCourses { get; set; } = new List<TeacherCourse>();

    }
}
