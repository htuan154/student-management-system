namespace StudentManagementSystem.Dtos.User
{
    public class UserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public string? EmployeeId { get; set; }
        public string? TeacherId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public string RoleName { get; set; } = string.Empty;

    }
}
