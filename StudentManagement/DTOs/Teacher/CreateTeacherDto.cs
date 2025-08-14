using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.DTOs.Teacher
{
    public class CreateTeacherDto
    {
        [Required]
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

        public DateTime? DateOfBirth { get; set; }

        public DateTime HireDate { get; set; } = DateTime.Today;

        [Column(TypeName = "decimal(12,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value")]
        public decimal? Salary { get; set; }
    }
}
