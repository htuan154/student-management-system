using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.User
{
    public class ChangePasswordDto
    {
        [Required]
        [StringLength(10)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
