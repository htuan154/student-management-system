namespace StudentManagementSystem.DTOs.Employee
{
    public class EmployeeResponseDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime HireDate { get; set; }
        public decimal? Salary { get; set; }
        public int UserCount { get; set; }
    }
}
