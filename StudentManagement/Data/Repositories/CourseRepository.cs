using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Course?> GetCourseWithEnrollmentsAsync(string courseId)
        {
            return await _context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
        {
            return await _context.Courses
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsCourseIdExistsAsync(string courseId)
        {
            return await _context.Courses.AnyAsync(c => c.CourseId == courseId);
        }

        public async Task<bool> IsCourseNameExistsAsync(string courseName, string? excludeCourseId = null)
        {
            return await _context.Courses.AnyAsync(c =>
                c.CourseName == courseName &&
                (excludeCourseId == null || c.CourseId != excludeCourseId));
        }

        public async Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm)
        {
            return await _context.Courses
                .Where(c => c.CourseName.Contains(searchTerm) || (c.Description != null && c.Description.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Course> Courses, int TotalCount)> GetPagedCoursesAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.CourseName.Contains(searchTerm) || (c.Description != null && c.Description.Contains(searchTerm)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var courses = await query
                .OrderBy(c => c.CourseName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (courses, totalCount);
        }

        public async Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(string department)
        {
            return await _context.Courses
                .Where(c => c.Department == department)
                .ToListAsync();
        }

        public async Task<int> GetEnrollmentCountInCourseAsync(string courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            return course?.Enrollments.Count ?? 0;
        }
    }
}
