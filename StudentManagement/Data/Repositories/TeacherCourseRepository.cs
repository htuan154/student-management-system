using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class TeacherCourseRepository : GenericRepository<TeacherCourse>, ITeacherCourseRepository
    {
        private readonly ApplicationDbContext _context;

        public TeacherCourseRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TeacherCourse>> GetByTeacherIdAsync(string teacherId)
        {
            return await _context.TeacherCourses.Where(tc => tc.TeacherId == teacherId).ToListAsync();
        }

        public async Task<IEnumerable<TeacherCourse>> GetByCourseIdAsync(string courseId)
        {
            return await _context.TeacherCourses.Where(tc => tc.CourseId == courseId).ToListAsync();
        }

        public async Task<IEnumerable<TeacherCourse>> SearchAsync(string searchTerm)
        {
            return await _context.TeacherCourses
                .Where(tc => tc.TeacherId.Contains(searchTerm) || tc.CourseId.Contains(searchTerm) || (tc.Semester != null && tc.Semester.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<(IEnumerable<TeacherCourse> TeacherCourses, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.TeacherCourses.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(tc => tc.TeacherId.Contains(searchTerm) || tc.CourseId.Contains(searchTerm) || (tc.Semester != null && tc.Semester.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var teacherCourses = await query
                .OrderBy(tc => tc.TeacherCourseId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (teacherCourses, totalCount);
        }
    }
}