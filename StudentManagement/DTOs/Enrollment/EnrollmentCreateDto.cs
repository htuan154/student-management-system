using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Enrollment
{
    public class EnrollmentCreateDto
    {
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
    }
}
