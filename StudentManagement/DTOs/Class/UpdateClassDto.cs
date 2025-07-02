using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Class
{
    public class UpdateClassDto
    {
        [Required]
        [StringLength(50)]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Major { get; set; } = string.Empty;

        [StringLength(20)]
        public string? AcademicYear { get; set; }

        [Range(1, 8)]
        public int? Semester { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
