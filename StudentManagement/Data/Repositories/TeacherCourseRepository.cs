using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class TeacherCourseRepository : GenericRepository<TeacherCourse>, ITeacherCourseRepository
    {
        public TeacherCourseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<TeacherCourse?> GetWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .Include(tc => tc.Semester)
                .FirstOrDefaultAsync(tc => tc.TeacherCourseId == id);
        }

        public async Task<IEnumerable<TeacherCourse>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .Include(tc => tc.Semester)
                .OrderBy(tc => tc.Semester.StartDate)
                .ThenBy(tc => tc.Course.CourseName)
                .ToListAsync();
        }

        public async Task<IEnumerable<TeacherCourse>> GetByTeacherIdAsync(string teacherId)
        {
            return await _context.TeacherCourses
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .Include(tc => tc.Semester) 
                .Where(tc => tc.TeacherId == teacherId && tc.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<TeacherCourse>> GetByCourseIdAsync(string courseId)
        {
            return await _dbSet
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .Include(tc => tc.Semester) // ✅ THÊM - Include Semester
                .Where(tc => tc.CourseId == courseId)
                .OrderByDescending(tc => tc.Semester.StartDate)
                .ThenBy(tc => tc.Teacher.FullName)
                .ToListAsync();
        }

        // ✅ THÊM - Method mới để get by SemesterId
        public async Task<IEnumerable<TeacherCourse>> GetBySemesterIdAsync(int semesterId)
        {
            return await _dbSet
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .Include(tc => tc.Semester)
                .Where(tc => tc.SemesterId == semesterId)
                .OrderBy(tc => tc.Course.CourseName)
                .ThenBy(tc => tc.Teacher.FullName)
                .ToListAsync();
        }

        // ✅ THÊM - Method để get active teacher courses
        public async Task<IEnumerable<TeacherCourse>> GetActiveTeacherCoursesAsync()
        {
            return await _dbSet
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .Include(tc => tc.Semester)
                .Where(tc => tc.IsActive && tc.Semester.IsActive) // ✅ Check both TeacherCourse and Semester active
                .OrderBy(tc => tc.Semester.StartDate)
                .ThenBy(tc => tc.Course.CourseName)
                .ToListAsync();
        }

        // ✅ SỬA - Search method với Semester navigation property
        public async Task<IEnumerable<TeacherCourse>> SearchAsync(string searchTerm)
        {
            return await _dbSet
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .Include(tc => tc.Semester)
                .Where(tc => tc.TeacherId.Contains(searchTerm) ||
                            tc.CourseId.Contains(searchTerm) ||
                            tc.Teacher.FullName.Contains(searchTerm) || // ✅ Search teacher name
                            tc.Course.CourseName.Contains(searchTerm) || // ✅ Search course name
                            (tc.Semester != null && tc.Semester.SemesterName.Contains(searchTerm)) || // ✅ SỬA - Search semester name
                            (tc.Semester != null && tc.Semester.AcademicYear.Contains(searchTerm))) // ✅ Search academic year
                .OrderBy(tc => tc.Course.CourseName)
                .ToListAsync();
        }

        // ✅ SỬA - GetPagedAsync với Semester navigation property
        public async Task<(IEnumerable<TeacherCourse> TeacherCourses, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .Include(tc => tc.Semester)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(tc => tc.TeacherId.Contains(searchTerm) ||
                                         tc.CourseId.Contains(searchTerm) ||
                                         tc.Teacher.FullName.Contains(searchTerm) ||
                                         tc.Course.CourseName.Contains(searchTerm) ||
                                         (tc.Semester != null && tc.Semester.SemesterName.Contains(searchTerm)) || // ✅ SỬA
                                         (tc.Semester != null && tc.Semester.AcademicYear.Contains(searchTerm))); // ✅ SỬA
            }

            var totalCount = await query.CountAsync();
            var teacherCourses = await query
                .OrderByDescending(tc => tc.Semester.StartDate) // ✅ Order by semester start date
                .ThenBy(tc => tc.Course.CourseName)
                .ThenBy(tc => tc.Teacher.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (teacherCourses, totalCount);
        }

        // ✅ THÊM - Validation methods
        public async Task<bool> IsTeacherCourseExistsAsync(string teacherId, string courseId, int semesterId, int? excludeId = null)
        {
            var query = _dbSet.Where(tc => tc.TeacherId == teacherId &&
                                          tc.CourseId == courseId &&
                                          tc.SemesterId == semesterId);

            if (excludeId.HasValue)
            {
                query = query.Where(tc => tc.TeacherCourseId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        // ✅ THÊM - Get teacher courses by semester and active status
        public async Task<IEnumerable<TeacherCourse>> GetTeacherCoursesBySemesterAsync(int semesterId, bool? isActive = null)
        {
            var query = _dbSet
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .Include(tc => tc.Semester)
                .Where(tc => tc.SemesterId == semesterId);

            if (isActive.HasValue)
            {
                query = query.Where(tc => tc.IsActive == isActive.Value);
            }

            return await query
                .OrderBy(tc => tc.Course.CourseName)
                .ThenBy(tc => tc.Teacher.FullName)
                .ToListAsync();
        }

        // ✅ THÊM - Get teacher workload
        public async Task<int> GetTeacherWorkloadAsync(string teacherId, int semesterId)
        {
            return await _dbSet
                .CountAsync(tc => tc.TeacherId == teacherId &&
                                 tc.SemesterId == semesterId &&
                                 tc.IsActive);
        }

        // ✅ THÊM - Get course instructors count
        public async Task<int> GetCourseInstructorsCountAsync(string courseId, int semesterId)
        {
            return await _dbSet
                .CountAsync(tc => tc.CourseId == courseId &&
                                 tc.SemesterId == semesterId &&
                                 tc.IsActive);
        }

        // ✅ Override GetAllAsync để include navigation properties
        public override async Task<IEnumerable<TeacherCourse>> GetAllAsync()
        {
            return await GetAllWithDetailsAsync();
        }

        // ✅ Override GetByIdAsync để include navigation properties
        public override async Task<TeacherCourse?> GetByIdAsync(object id)
        {
            return await GetWithDetailsAsync((int)id);
        }
    }
}
