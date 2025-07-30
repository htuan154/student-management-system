using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Course
{
    public class CourseCreateDto
    {
        [Required]
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

        public bool IsActive { get; set; } = true;
    }
}
