using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Employee;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();
            return employees.Select(MapToResponseDto);
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(string employeeId)
        {
            var employee = await _employeeRepository.GetEmployeeWithUsersAsync(employeeId);
            return employee == null ? null : MapToResponseDto(employee);
        }

        public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto)
        {
            // Check if EmployeeId already exists
            if (await _employeeRepository.IsEmployeeIdExistsAsync(createEmployeeDto.EmployeeId))
            {
                throw new InvalidOperationException("Employee ID already exists");
            }

            // Check if Email already exists
            if (await _employeeRepository.IsEmailExistsAsync(createEmployeeDto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            var employee = MapToEmployee(createEmployeeDto);
            var createdEmployee = await _employeeRepository.AddAsync(employee);

            // Get employee with user information
            var employeeWithUsers = await _employeeRepository.GetEmployeeWithUsersAsync(createdEmployee.EmployeeId);
            return MapToResponseDto(employeeWithUsers!);
        }

        public async Task<EmployeeResponseDto?> UpdateEmployeeAsync(string employeeId, UpdateEmployeeDto updateEmployeeDto)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                return null;

            // Check if Email already exists (excluding current employee)
            if (await _employeeRepository.IsEmailExistsAsync(updateEmployeeDto.Email, employeeId))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Update employee properties
            employee.FullName = updateEmployeeDto.FullName;
            employee.Email = updateEmployeeDto.Email;
            employee.PhoneNumber = updateEmployeeDto.PhoneNumber;
            employee.Department = updateEmployeeDto.Department;
            employee.Position = updateEmployeeDto.Position;
            employee.DateOfBirth = updateEmployeeDto.DateOfBirth;
            employee.HireDate = updateEmployeeDto.HireDate;
            employee.Salary = updateEmployeeDto.Salary;

            await _employeeRepository.UpdateAsync(employee);

            // Get updated employee with user information
            var updatedEmployee = await _employeeRepository.GetEmployeeWithUsersAsync(employeeId);
            return MapToResponseDto(updatedEmployee!);
        }

        public async Task<bool> DeleteEmployeeAsync(string employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                return false;

            await _employeeRepository.DeleteAsync(employee);
            return true;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetEmployeesByDepartmentAsync(string department)
        {
            var employees = await _employeeRepository.GetEmployeesByDepartmentAsync(department);
            return employees.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetEmployeesByPositionAsync(string position)
        {
            var employees = await _employeeRepository.GetEmployeesByPositionAsync(position);
            return employees.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<EmployeeResponseDto>> SearchEmployeesAsync(string searchTerm)
        {
            var employees = await _employeeRepository.SearchEmployeesAsync(searchTerm);
            return employees.Select(MapToResponseDto);
        }

        public async Task<(IEnumerable<EmployeeResponseDto> Employees, int TotalCount)> GetPagedEmployeesAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (employees, totalCount) = await _employeeRepository.GetPagedEmployeesAsync(pageNumber, pageSize, searchTerm);
            var employeeDtos = employees.Select(MapToResponseDto);
            return (employeeDtos, totalCount);
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetEmployeesBySalaryRangeAsync(decimal? minSalary, decimal? maxSalary)
        {
            var employees = await _employeeRepository.GetEmployeesBySalaryRangeAsync(minSalary, maxSalary);
            return employees.Select(MapToResponseDto);
        }

        public async Task<bool> IsEmailExistsAsync(string email, string? excludeEmployeeId = null)
        {
            return await _employeeRepository.IsEmailExistsAsync(email, excludeEmployeeId);
        }

        public async Task<bool> IsEmployeeIdExistsAsync(string employeeId)
        {
            return await _employeeRepository.IsEmployeeIdExistsAsync(employeeId);
        }

        public async Task<IEnumerable<string>> GetDistinctDepartmentsAsync()
        {
            return await _employeeRepository.GetDistinctDepartmentsAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctPositionsAsync()
        {
            return await _employeeRepository.GetDistinctPositionsAsync();
        }

        // Mapping methods
        private EmployeeResponseDto MapToResponseDto(Employee employee)
        {
            return new EmployeeResponseDto
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                Department = employee.Department,
                Position = employee.Position,
                DateOfBirth = employee.DateOfBirth,
                HireDate = employee.HireDate,
                Salary = employee.Salary,
                UserCount = employee.Users?.Count ?? 0
            };
        }

        private Employee MapToEmployee(CreateEmployeeDto dto)
        {
            return new Employee
            {
                EmployeeId = dto.EmployeeId,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Department = dto.Department,
                Position = dto.Position,
                DateOfBirth = dto.DateOfBirth,
                HireDate = dto.HireDate,
                Salary = dto.Salary
            };
        }
    }
}
