using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsWithScoresAsync(string studentId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.Semester.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(string studentId)
        {
            return await _dbSet
                .Include(e => e.Course)
                .Include(e => e.Student)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(string courseId)
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();
        }

        // ✅ THÊM - GetByTeacherIdAsync ở đây để grouping logic
        public async Task<IEnumerable<Enrollment>> GetByTeacherIdAsync(string teacherId)
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .Where(e => e.TeacherCourse != null && e.TeacherCourse.TeacherId == teacherId)
                .OrderByDescending(e => e.Semester.StartDate)
                .ThenBy(e => e.Course.CourseName)
                .ThenBy(e => e.Student.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetByTeacherCourseIdAsync(int teacherCourseId)
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .Where(e => e.TeacherCourseId == teacherCourseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetBySemesterIdAsync(int semesterId)
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .Where(e => e.SemesterId == semesterId)
                .ToListAsync();
        }

        // ✅ THÊM - GetByStatusAsync ở đây
        public async Task<IEnumerable<Enrollment>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .Where(e => e.Status == status)
                .OrderByDescending(e => e.Semester.StartDate)
                .ThenBy(e => e.Course.CourseName)
                .ThenBy(e => e.Student.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> SearchAsync(string searchTerm)
        {
            return await _dbSet
                .Include(e => e.Course)
                .Include(e => e.Student)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc!.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .Where(e => e.StudentId != null && e.StudentId.Contains(searchTerm) ||
                           e.CourseId != null && e.CourseId.Contains(searchTerm) ||
                           e.Student != null && e.Student.FullName.Contains(searchTerm) ||
                           e.Course != null && e.Course.CourseName.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Enrollment> Enrollments, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.StudentId.Contains(searchTerm) ||
                                        e.CourseId.Contains(searchTerm) ||
                                        (e.Status != null && e.Status.Contains(searchTerm)) ||
                                        e.Student.FullName.Contains(searchTerm) ||
                                        e.Course.CourseName.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var enrollments = await query
                .OrderByDescending(e => e.Semester.StartDate)
                .ThenBy(e => e.EnrollmentId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (enrollments, totalCount);
        }

        public async Task<IEnumerable<Enrollment>> GetUnscoredAsync()
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .Where(e => e.Score == null) 
                .OrderBy(e => e.Semester.StartDate)
                .ThenBy(e => e.Course.CourseName)
                .ThenBy(e => e.Student.FullName)
                .ToListAsync();
        }


        public async Task<IEnumerable<Enrollment>> GetUnscoredEnrollmentsByTeacherCourseAsync(int teacherCourseId)
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score) // ✅ Include để check null
                .Where(e => e.TeacherCourseId == teacherCourseId && e.Score == null)
                .OrderBy(e => e.Student.FullName)
                .ToListAsync();
        }


        public async Task<IEnumerable<Enrollment>> GetUnscoredEnrollmentsForClassAsync(string courseId, int teacherCourseId)
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .Where(e => e.CourseId == courseId &&
                           e.TeacherCourseId == teacherCourseId &&
                           e.Score == null)
                .OrderBy(e => e.Student.FullName)
                .ToListAsync();
        }


        public async Task<bool> IsStudentEnrolledInCourseAsync(string studentId, string courseId, int semesterId)
        {
            return await _dbSet
                .AnyAsync(e => e.StudentId == studentId && 
                              e.CourseId == courseId && 
                              e.SemesterId == semesterId);
        }

        public async Task<bool> CanDeleteEnrollmentAsync(int enrollmentId)
        {
            // Check if enrollment has scores
            var hasScores = await _context.Scores.AnyAsync(s => s.EnrollmentId == enrollmentId);
            return !hasScores;
        }

        
        public async Task<bool> IsStudentEnrolledAsync(string studentId, string courseId, int semesterId)
        {
            return await IsStudentEnrolledInCourseAsync(studentId, courseId, semesterId);
        }


        public async Task<int> GetEnrollmentCountByCourseAsync(string courseId, int semesterId)
        {
            return await _dbSet
                .CountAsync(e => e.CourseId == courseId && e.SemesterId == semesterId);
        }

        public async Task<int> GetEnrollmentCountByTeacherCourseAsync(int teacherCourseId)
        {
            return await _dbSet
                .CountAsync(e => e.TeacherCourseId == teacherCourseId);
        }

        public async Task<int> GetActiveEnrollmentCountBySemesterAsync(int semesterId)
        {
            return await _dbSet
                .CountAsync(e => e.SemesterId == semesterId && e.Status == "Active");
        }
        

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAndSemesterAsync(string studentId, int semesterId)
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .Where(e => e.StudentId == studentId && e.SemesterId == semesterId)
                .OrderBy(e => e.Course.CourseName)
                .ToListAsync();
        }

       
        public override async Task<IEnumerable<Enrollment>> GetAllAsync()
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .OrderByDescending(e => e.Semester.StartDate)
                .ThenBy(e => e.Course.CourseName)
                .ToListAsync();
        }


        public override async Task<Enrollment?> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(e => e.Semester)
                .Include(e => e.Score)
                .FirstOrDefaultAsync(e => e.EnrollmentId == (int)id);
        }
    }
}
