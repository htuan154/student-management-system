using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(string studentId)
        {
            return await _context.Enrollments.Where(e => e.StudentId == studentId).ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(string courseId)
        {
            return await _context.Enrollments.Where(e => e.CourseId == courseId).ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> SearchAsync(string searchTerm)
        {
            return await _context.Enrollments
                .Where(e => e.StudentId.Contains(searchTerm) || e.CourseId.Contains(searchTerm) || (e.Status != null && e.Status.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Enrollment> Enrollments, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.Enrollments.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.StudentId.Contains(searchTerm) || e.CourseId.Contains(searchTerm) || (e.Status != null && e.Status.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var enrollments = await query
                .OrderBy(e => e.EnrollmentId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (enrollments, totalCount);
        }
        public async Task<IEnumerable<Enrollment>> GetUnscoredAsync()
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => !_context.Scores.Any(s =>
                    s.EnrollmentId == e.EnrollmentId &&
                    (s.ProcessScore != null || s.MidtermScore != null || s.FinalScore != null)
                ))
                .ToListAsync();
        }
    }
}
