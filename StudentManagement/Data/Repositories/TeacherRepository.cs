using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Teacher?> GetTeacherWithUsersAsync(string teacherId)
        {
            return await _dbSet
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);
        }

        public async Task<Teacher?> GetTeacherWithCoursesAsync(string teacherId)
        {
            return await _dbSet
                .Include(t => t.TeacherCourses)
                    .ThenInclude(tc => tc.Course)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);
        }

        public async Task<Teacher?> GetTeacherWithEnrollmentsAsync(string teacherId)
        {
            return await _dbSet
                .Include(t => t.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(t => t.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);
        }

        public async Task<Teacher?> GetTeacherFullDetailsAsync(string teacherId)
        {
            return await _dbSet
                .Include(t => t.Users)
                .Include(t => t.TeacherCourses)
                    .ThenInclude(tc => tc.Course)
                .Include(t => t.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(t => t.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);
        }

        public async Task<IEnumerable<Teacher>> GetTeachersByDepartmentAsync(string department)
        {
            return await _dbSet
                .Where(t => t.Department == department)
                .ToListAsync();
        }

        public async Task<IEnumerable<Teacher>> GetTeachersByDegreeAsync(string degree)
        {
            return await _dbSet
                .Where(t => t.Degree == degree)
                .ToListAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, string? excludeTeacherId = null)
        {
            var query = _dbSet.Where(t => t.Email == email);

            if (!string.IsNullOrEmpty(excludeTeacherId))
            {
                query = query.Where(t => t.TeacherId != excludeTeacherId);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsTeacherIdExistsAsync(string teacherId)
        {
            return await _dbSet.AnyAsync(t => t.TeacherId == teacherId);
        }

        public async Task<IEnumerable<Teacher>> SearchTeachersAsync(string searchTerm)
        {
            return await _dbSet
                .Where(t => t.FullName.Contains(searchTerm) ||
                           t.TeacherId.Contains(searchTerm) ||
                           t.Email.Contains(searchTerm) ||
                           (t.Department != null && t.Department.Contains(searchTerm)) ||
                           (t.Degree != null && t.Degree.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Teacher> Teachers, int TotalCount)> GetPagedTeachersAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => t.FullName.Contains(searchTerm) ||
                                        t.TeacherId.Contains(searchTerm) ||
                                        t.Email.Contains(searchTerm) ||
                                        (t.Department != null && t.Department.Contains(searchTerm)) ||
                                        (t.Degree != null && t.Degree.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var teachers = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (teachers, totalCount);
        }

        public async Task<IEnumerable<Teacher>> GetTeachersBySalaryRangeAsync(decimal? minSalary, decimal? maxSalary)
        {
            var query = _dbSet.AsQueryable();

            if (minSalary.HasValue)
            {
                query = query.Where(t => t.Salary >= minSalary.Value);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(t => t.Salary <= maxSalary.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctDepartmentsAsync()
        {
            return await _dbSet
                .Where(t => t.Department != null)
                .Select(t => t.Department!)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctDegreesAsync()
        {
            return await _dbSet
                .Where(t => t.Degree != null)
                .Select(t => t.Degree!)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Teacher>> GetTeachersWithActiveCourses()
        {
            return await _dbSet
                .Include(t => t.TeacherCourses)
                    .ThenInclude(tc => tc.Course)
                .Where(t => t.TeacherCourses.Any())
                .ToListAsync();
        }

        public async Task<int> GetTeacherCourseCountAsync(string teacherId)
        {
            return await _dbSet
                .Where(t => t.TeacherId == teacherId)
                .SelectMany(t => t.TeacherCourses)
                .CountAsync();
        }

        public async Task<int> GetTeacherEnrollmentCountAsync(string teacherId)
        {
            return await _dbSet
                .Where(t => t.TeacherId == teacherId)
                .SelectMany(t => t.Enrollments)
                .CountAsync();
        }
        public async Task<IEnumerable<Teacher>> GetTeachersByCourseIdAsync(string courseId)
        {
            return await _dbSet
                .Where(t => t.TeacherCourses.Any(tc => tc.CourseId == courseId))
                .ToListAsync();
        }
    }
}
