namespace StudentManagementSystem.DTOs.Student
{
    public class StudentResponseDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string ClassId { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
    }
}
