using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class SemesterRepository : GenericRepository<Semester>, ISemesterRepository
    {
        public SemesterRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Semester>> GetActiveSemstersAsync()
        {
            return await _dbSet
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<Semester?> GetSemesterWithClassesAsync(int semesterId)
        {
            return await _dbSet
                .Include(s => s.Classes)
                .FirstOrDefaultAsync(s => s.SemesterId == semesterId);
        }

        public async Task<Semester?> GetSemesterWithTeacherCoursesAsync(int semesterId)
        {
            return await _dbSet
                .Include(s => s.TeacherCourses)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourses)
                    .ThenInclude(tc => tc.Course)
                .FirstOrDefaultAsync(s => s.SemesterId == semesterId);
        }

        public async Task<Semester?> GetSemesterWithEnrollmentsAsync(int semesterId)
        {
            return await _dbSet
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.SemesterId == semesterId);
        }

        public async Task<bool> IsSemesterNameExistsAsync(string semesterName, string academicYear, int? excludeSemesterId = null)
        {
            var query = _dbSet.Where(s => s.SemesterName == semesterName && s.AcademicYear == academicYear);

            if (excludeSemesterId.HasValue)
            {
                query = query.Where(s => s.SemesterId != excludeSemesterId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Semester>> GetSemestersByAcademicYearAsync(string academicYear)
        {
            return await _dbSet
                .Where(s => s.AcademicYear == academicYear)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Semester> Semesters, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.SemesterName.Contains(searchTerm) ||
                                        s.AcademicYear.Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var semesters = await query
                .OrderByDescending(s => s.StartDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (semesters, totalCount);
        }

        public async Task<bool> CanDeleteSemesterAsync(int semesterId)
        {
            var hasClasses = await _context.Classes.AnyAsync(c => c.SemesterId == semesterId);
            var hasTeacherCourses = await _context.TeacherCourses.AnyAsync(tc => tc.SemesterId == semesterId);
            var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.SemesterId == semesterId);

            return !hasClasses && !hasTeacherCourses && !hasEnrollments;
        }
    }
}
