using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.TeacherCourse
{
    public class TeacherCourseCreateDto
    {
        [Required]
        public string TeacherId { get; set; } = string.Empty;

        [Required]
        public string CourseId { get; set; } = string.Empty;

        [Required]
        public int SemesterId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
