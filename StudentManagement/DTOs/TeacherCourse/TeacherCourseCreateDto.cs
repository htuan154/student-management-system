using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Dtos.TeacherCourse
{
    public class TeacherCourseCreateDto
    {
        [Required]
        [StringLength(10)]
        public string TeacherId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CourseId { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Semester { get; set; }

        public int? Year { get; set; }

        public bool IsActive { get; set; } = true;
    }
}