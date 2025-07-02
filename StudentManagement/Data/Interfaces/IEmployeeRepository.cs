using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<Employee?> GetEmployeeWithUsersAsync(string employeeId);
        Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department);
        Task<bool> IsEmailExistsAsync(string email, string? excludeEmployeeId = null);
        Task<bool> IsEmployeeIdExistsAsync(string employeeId);
        Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm);
        Task<(IEnumerable<Employee> Employees, int TotalCount)> GetPagedEmployeesAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<Employee>> GetEmployeesByPositionAsync(string position);
        Task<IEnumerable<Employee>> GetEmployeesBySalaryRangeAsync(decimal? minSalary, decimal? maxSalary);
        Task<IEnumerable<string>> GetDistinctDepartmentsAsync();
        Task<IEnumerable<string>> GetDistinctPositionsAsync();
    }
}
