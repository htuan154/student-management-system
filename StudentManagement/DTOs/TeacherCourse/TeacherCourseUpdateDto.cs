using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.TeacherCourse
{
    public class TeacherCourseUpdateDto
    {
        [Required]
        public int TeacherCourseId { get; set; }

        [Required]
        [StringLength(10)]
        public string TeacherId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CourseId { get; set; } = string.Empty;

        [Required]
        public int SemesterId { get; set; }

        public bool IsActive { get; set; }
    }
}
