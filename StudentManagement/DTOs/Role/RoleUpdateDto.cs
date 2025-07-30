using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Role
{
    public class RoleUpdateDto
    {
        [Required]
        [StringLength(10)]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }
    }
}
