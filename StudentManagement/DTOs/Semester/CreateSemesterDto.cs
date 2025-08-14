using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Semester
{
    public class CreateSemesterDto
    {
        [Required]
        [StringLength(50)]
        public string SemesterName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
