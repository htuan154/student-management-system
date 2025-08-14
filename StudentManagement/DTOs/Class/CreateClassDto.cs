using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Class
{
    public class CreateClassDto
    {
        [Required]
        [StringLength(20)]
        public string ClassId { get; set; } = string.Empty;

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
