using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Employee
{
    public class CreateEmployeeDto
    {
        [Required]
        [StringLength(10)]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        public string? Department { get; set; }

        [StringLength(50)]
        public string? Position { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime HireDate { get; set; } = DateTime.Today;

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value")]
        public decimal? Salary { get; set; }
    }
}
