using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Employee;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<EmployeeService> _logger;
        private const string EmployeeCachePrefix = "employee";

        public EmployeeService(IEmployeeRepository employeeRepository, ICacheService cacheService, ILogger<EmployeeService> logger)
        {
            _employeeRepository = employeeRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync()
        {
            _logger.LogInformation("Getting all employees.");
            var employees = await _employeeRepository.GetAllAsync();
            return employees.Select(MapToResponseDto);
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(string employeeId)
        {
            _logger.LogInformation("Getting employee by ID: {EmployeeId}", employeeId);
            var cacheKey = $"{EmployeeCachePrefix}:{employeeId}";

            var cachedEmployee = await _cacheService.GetDataAsync<EmployeeResponseDto>(cacheKey);
            if (cachedEmployee != null)
            {
                _logger.LogInformation("Employee {EmployeeId} found in cache.", employeeId);
                return cachedEmployee;
            }

            _logger.LogInformation("Employee {EmployeeId} not in cache. Fetching from database.", employeeId);
            var employee = await _employeeRepository.GetEmployeeWithUsersAsync(employeeId);
            if (employee == null) return null;

            var employeeDto = MapToResponseDto(employee);

            await _cacheService.SetDataAsync(cacheKey, employeeDto, System.DateTimeOffset.Now.AddMinutes(5));
            return employeeDto;
        }

        public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto)
        {
            _logger.LogInformation("Creating new employee with ID: {EmployeeId}", createEmployeeDto.EmployeeId);
            if (await _employeeRepository.IsEmployeeIdExistsAsync(createEmployeeDto.EmployeeId) || await _employeeRepository.IsEmailExistsAsync(createEmployeeDto.Email))
            {
                throw new InvalidOperationException("Employee ID or Email already exists.");
            }

            var employee = MapToEmployee(createEmployeeDto);
            var createdEmployee = await _employeeRepository.AddAsync(employee);

            _logger.LogInformation("Employee {EmployeeId} created successfully.", createdEmployee.EmployeeId);

            // Invalidate caches for lists of employees
            await InvalidateEmployeeListCaches();

            var employeeWithUsers = await _employeeRepository.GetEmployeeWithUsersAsync(createdEmployee.EmployeeId);
            return MapToResponseDto(employeeWithUsers!);
        }

        public async Task<EmployeeResponseDto?> UpdateEmployeeAsync(string employeeId, UpdateEmployeeDto updateEmployeeDto)
        {
            _logger.LogInformation("Updating employee with ID: {EmployeeId}", employeeId);
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                 _logger.LogWarning("Update failed. Employee with ID {EmployeeId} not found.", employeeId);
                return null;
            }

            if (await _employeeRepository.IsEmailExistsAsync(updateEmployeeDto.Email, employeeId))
            {
                throw new InvalidOperationException("Email already exists");
            }

            employee.FullName = updateEmployeeDto.FullName;
            employee.Email = updateEmployeeDto.Email;
            employee.PhoneNumber = updateEmployeeDto.PhoneNumber;
            employee.Department = updateEmployeeDto.Department;
            employee.Position = updateEmployeeDto.Position;
            employee.DateOfBirth = updateEmployeeDto.DateOfBirth;
            employee.HireDate = updateEmployeeDto.HireDate;
            employee.Salary = updateEmployeeDto.Salary;

            await _employeeRepository.UpdateAsync(employee);
            _logger.LogInformation("Employee {EmployeeId} updated successfully.", employeeId);

            // Invalidate caches
            await _cacheService.RemoveDataAsync($"{EmployeeCachePrefix}:{employeeId}");
            await InvalidateEmployeeListCaches();
            _logger.LogInformation("Cache for employee {EmployeeId} and employee lists invalidated.", employeeId);

            var updatedEmployee = await _employeeRepository.GetEmployeeWithUsersAsync(employeeId);
            return MapToResponseDto(updatedEmployee!);
        }

        public async Task<bool> DeleteEmployeeAsync(string employeeId)
        {
            _logger.LogInformation("Deleting employee with ID: {EmployeeId}", employeeId);
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Delete failed. Employee with ID {EmployeeId} not found.", employeeId);
                return false;
            }

            await _employeeRepository.DeleteAsync(employee);
            _logger.LogInformation("Employee {EmployeeId} deleted successfully.", employeeId);

            // Invalidate caches
            await _cacheService.RemoveDataAsync($"{EmployeeCachePrefix}:{employeeId}");
            await InvalidateEmployeeListCaches();
            _logger.LogInformation("Cache for employee {EmployeeId} and employee lists invalidated.", employeeId);

            return true;
        }

        private Task InvalidateEmployeeListCaches()
        {
            // This is a placeholder for a more sophisticated strategy
            // For now, we assume we might have cached lists of departments or positions
            var tasks = new List<Task>
            {
                _cacheService.RemoveDataAsync($"{EmployeeCachePrefix}:distinct_departments"),
                _cacheService.RemoveDataAsync($"{EmployeeCachePrefix}:distinct_positions")
            };
            return Task.WhenAll(tasks);
        }

        // ... (Other specific Get and helper methods)

        #region Other Public Methods
        public async Task<IEnumerable<EmployeeResponseDto>> GetEmployeesByDepartmentAsync(string department)
        {
            _logger.LogInformation("Getting employees by department: {Department}", department);
            var employees = await _employeeRepository.GetEmployeesByDepartmentAsync(department);
            return employees.Select(MapToResponseDto);
        }
        public async Task<IEnumerable<EmployeeResponseDto>> GetEmployeesByPositionAsync(string position)
        {
            _logger.LogInformation("Getting employees by position: {Position}", position);
            var employees = await _employeeRepository.GetEmployeesByPositionAsync(position);
            return employees.Select(MapToResponseDto);
        }
        public async Task<IEnumerable<EmployeeResponseDto>> SearchEmployeesAsync(string searchTerm)
        {
            _logger.LogInformation("Searching employees with term: {SearchTerm}", searchTerm);
            var employees = await _employeeRepository.SearchEmployeesAsync(searchTerm);
            return employees.Select(MapToResponseDto);
        }
        public async Task<(IEnumerable<EmployeeResponseDto> Employees, int TotalCount)> GetPagedEmployeesAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            _logger.LogInformation("Getting paged employees. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            var (employees, totalCount) = await _employeeRepository.GetPagedEmployeesAsync(pageNumber, pageSize, searchTerm);
            return (employees.Select(MapToResponseDto), totalCount);
        }
        public async Task<IEnumerable<EmployeeResponseDto>> GetEmployeesBySalaryRangeAsync(decimal? minSalary, decimal? maxSalary)
        {
            _logger.LogInformation("Getting employees by salary range.");
            var employees = await _employeeRepository.GetEmployeesBySalaryRangeAsync(minSalary, maxSalary);
            return employees.Select(MapToResponseDto);
        }
        public Task<bool> IsEmailExistsAsync(string email, string? excludeEmployeeId = null) => _employeeRepository.IsEmailExistsAsync(email, excludeEmployeeId);
        public Task<bool> IsEmployeeIdExistsAsync(string employeeId) => _employeeRepository.IsEmployeeIdExistsAsync(employeeId);

        public async Task<IEnumerable<string>> GetDistinctDepartmentsAsync()
        {
            const string cacheKey = $"{EmployeeCachePrefix}:distinct_departments";
            var cachedData = await _cacheService.GetDataAsync<IEnumerable<string>>(cacheKey);
            if(cachedData != null)
            {
                _logger.LogInformation("Distinct departments retrieved from cache.");
                return cachedData;
            }

            var data = await _employeeRepository.GetDistinctDepartmentsAsync();
            await _cacheService.SetDataAsync(cacheKey, data, System.DateTimeOffset.Now.AddHours(1));
            return data;
        }

        public async Task<IEnumerable<string>> GetDistinctPositionsAsync()
        {
            const string cacheKey = $"{EmployeeCachePrefix}:distinct_positions";
            var cachedData = await _cacheService.GetDataAsync<IEnumerable<string>>(cacheKey);
            if(cachedData != null)
            {
                _logger.LogInformation("Distinct positions retrieved from cache.");
                return cachedData;
            }

            var data = await _employeeRepository.GetDistinctPositionsAsync();
            await _cacheService.SetDataAsync(cacheKey, data, System.DateTimeOffset.Now.AddHours(1));
            return data;
        }
        #endregion

        #region Mapping Methods
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
        #endregion
    }
}
