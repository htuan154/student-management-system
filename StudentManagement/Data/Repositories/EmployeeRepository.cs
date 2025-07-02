using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Employee?> GetEmployeeWithUsersAsync(string employeeId)
        {
            return await _dbSet
                .Include(e => e.Users)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department)
        {
            return await _dbSet
                .Where(e => e.Department == department)
                .ToListAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, string? excludeEmployeeId = null)
        {
            var query = _dbSet.Where(e => e.Email == email);

            if (!string.IsNullOrEmpty(excludeEmployeeId))
            {
                query = query.Where(e => e.EmployeeId != excludeEmployeeId);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsEmployeeIdExistsAsync(string employeeId)
        {
            return await _dbSet.AnyAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm)
        {
            return await _dbSet
                .Where(e => e.FullName.Contains(searchTerm) ||
                           e.EmployeeId.Contains(searchTerm) ||
                           e.Email.Contains(searchTerm) ||
                           (e.Department != null && e.Department.Contains(searchTerm)) ||
                           (e.Position != null && e.Position.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Employee> Employees, int TotalCount)> GetPagedEmployeesAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.FullName.Contains(searchTerm) ||
                                        e.EmployeeId.Contains(searchTerm) ||
                                        e.Email.Contains(searchTerm) ||
                                        (e.Department != null && e.Department.Contains(searchTerm)) ||
                                        (e.Position != null && e.Position.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var employees = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (employees, totalCount);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByPositionAsync(string position)
        {
            return await _dbSet
                .Where(e => e.Position == position)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesBySalaryRangeAsync(decimal? minSalary, decimal? maxSalary)
        {
            var query = _dbSet.AsQueryable();

            if (minSalary.HasValue)
            {
                query = query.Where(e => e.Salary >= minSalary.Value);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(e => e.Salary <= maxSalary.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctDepartmentsAsync()
        {
            return await _dbSet
                .Where(e => e.Department != null)
                .Select(e => e.Department!)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctPositionsAsync()
        {
            return await _dbSet
                .Where(e => e.Position != null)
                .Select(e => e.Position!)
                .Distinct()
                .ToListAsync();
        }
    }
}
