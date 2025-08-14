using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Enrollment
{
    public class EnrollmentCreateDto
    {
        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public string CourseId { get; set; } = string.Empty;


        [Required]
        public int TeacherCourseId { get; set; }

        [Required]
        public int SemesterId { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
    }
}
