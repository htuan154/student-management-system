using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.User
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(10)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Password { get; set; }

        [Required]
        [StringLength(10)]
        public string RoleId { get; set; } = string.Empty;

        [StringLength(10)]
        public string? StudentId { get; set; }

        [StringLength(10)]
        public string? EmployeeId { get; set; }

        [StringLength(10)]
        public string? TeacherId { get; set; }

        public bool IsActive { get; set; }
    }
}
