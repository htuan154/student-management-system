using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Student
{
    public class CreateStudentDto
    {
        [Required]
        [StringLength(10)]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [Required]
        [StringLength(10)]
        public string ClassId { get; set; } = string.Empty;
    }
}
