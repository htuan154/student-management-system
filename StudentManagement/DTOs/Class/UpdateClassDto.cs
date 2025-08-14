using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Class
{
    public class UpdateClassDto
    {
        [Required]
        [StringLength(100)]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Major { get; set; } = string.Empty;

        [Required]
        public int SemesterId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
