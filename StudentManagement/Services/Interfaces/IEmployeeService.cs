using StudentManagementSystem.DTOs.Employee;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync();
        Task<EmployeeResponseDto?> GetEmployeeByIdAsync(string employeeId);
        Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto);
        Task<EmployeeResponseDto?> UpdateEmployeeAsync(string employeeId, UpdateEmployeeDto updateEmployeeDto);
        Task<bool> DeleteEmployeeAsync(string employeeId);
        Task<IEnumerable<EmployeeResponseDto>> GetEmployeesByDepartmentAsync(string department);
        Task<IEnumerable<EmployeeResponseDto>> GetEmployeesByPositionAsync(string position);
        Task<IEnumerable<EmployeeResponseDto>> SearchEmployeesAsync(string searchTerm);
        Task<(IEnumerable<EmployeeResponseDto> Employees, int TotalCount)> GetPagedEmployeesAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<EmployeeResponseDto>> GetEmployeesBySalaryRangeAsync(decimal? minSalary, decimal? maxSalary);
        Task<bool> IsEmailExistsAsync(string email, string? excludeEmployeeId = null);
        Task<bool> IsEmployeeIdExistsAsync(string employeeId);
        Task<IEnumerable<string>> GetDistinctDepartmentsAsync();
        Task<IEnumerable<string>> GetDistinctPositionsAsync();
    }
}
