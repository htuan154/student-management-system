using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class ScoreRepository : GenericRepository<Score>, IScoreRepository
    {
        public ScoreRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Score?> GetByEnrollmentIdAsync(int enrollmentId)
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .FirstOrDefaultAsync(s => s.EnrollmentId == enrollmentId);
        }

        public async Task<IEnumerable<Score>> GetByStudentIdAsync(string studentId)
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .Where(s => s.Enrollment.StudentId == studentId)
                .OrderByDescending(s => s.Enrollment.Semester.StartDate) // ✅ Order by semester
                .ToListAsync();
        }


        public async Task<IEnumerable<Score>> GetByTeacherAndCourseAsync(string teacherId, string courseId)
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .Where(s => s.Enrollment.TeacherCourse != null &&
                           s.Enrollment.TeacherCourse.TeacherId == teacherId &&
                           s.Enrollment.CourseId == courseId)
                .OrderBy(s => s.Enrollment.Student.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Score>> GetByTeacherAndSubjectAsync(string teacherId, string courseId)
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .Where(s => s.Enrollment.TeacherCourse != null &&
                           s.Enrollment.TeacherCourse.TeacherId == teacherId &&
                           s.Enrollment.CourseId == courseId)
                .Select(s => new Score // ⬅ THÊM SELECT để tránh computed column
                {
                    ScoreId = s.ScoreId,
                    EnrollmentId = s.EnrollmentId,
                    ProcessScore = s.ProcessScore,
                    MidtermScore = s.MidtermScore,
                    FinalScore = s.FinalScore,

                    Enrollment = s.Enrollment
                })
                .OrderBy(s => s.Enrollment.Student.FullName)
                .ToListAsync(); // ⬅ Line 67
        }

        public async Task<IEnumerable<Score>> GetByTeacherCourseIdAsync(int teacherCourseId)
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .Where(s => s.Enrollment.TeacherCourseId == teacherCourseId)
                .OrderBy(s => s.Enrollment.Student.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Score>> GetByTeacherIdAsync(string teacherId)
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .Where(s => s.Enrollment.TeacherCourse != null &&
                        s.Enrollment.TeacherCourse.TeacherId == teacherId)
                .OrderByDescending(s => s.Enrollment.Semester.StartDate)
                .ThenBy(s => s.Enrollment.Course.CourseName)
                .ThenBy(s => s.Enrollment.Student.FullName)
                .ToListAsync();
        }


        public async Task<IEnumerable<Score>> GetByCourseIdAsync(string courseId)
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .Where(s => s.Enrollment.CourseId == courseId)
                .OrderByDescending(s => s.Enrollment.Semester.StartDate)
                .ThenBy(s => s.Enrollment.Student.FullName)
                .ToListAsync();
        }


        public async Task<IEnumerable<Score>> GetBySemesterIdAsync(int semesterId)
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .Where(s => s.Enrollment.SemesterId == semesterId)
                .OrderBy(s => s.Enrollment.Course.CourseName)
                .ThenBy(s => s.Enrollment.Student.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Score>> SearchScoresAsync(string searchTerm)
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .Where(s => s.EnrollmentId.ToString().Contains(searchTerm) ||
                        s.Enrollment.Student.FullName.Contains(searchTerm) ||
                        s.Enrollment.Student.StudentId.Contains(searchTerm) ||
                        s.Enrollment.Course.CourseName.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Score> Scores, int TotalCount)> GetPagedScoresAsync(
            int pageNumber, int pageSize, string? searchTerm = null, string? courseId = null, int? semesterId = null)
        {
            var query = _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.EnrollmentId.ToString().Contains(searchTerm) ||
                                        s.Enrollment.Student.FullName.Contains(searchTerm) ||
                                        s.Enrollment.Student.StudentId.Contains(searchTerm) ||
                                        s.Enrollment.Course.CourseName.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(courseId))
            {
                query = query.Where(s => s.Enrollment.CourseId == courseId);
            }

            if (semesterId.HasValue)
            {
                query = query.Where(s => s.Enrollment.SemesterId == semesterId.Value);
            }

            var totalCount = await query.CountAsync();
            var scores = await query
                .OrderByDescending(s => s.Enrollment.Semester.StartDate)
                .ThenBy(s => s.Enrollment.Course.CourseName)
                .ThenBy(s => s.Enrollment.Student.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (scores, totalCount);
        }

        public async Task<decimal> GetAverageScoreByStudentAsync(string studentId)
        {
            var scores = await _dbSet
                .Where(s => s.Enrollment.StudentId == studentId && s.TotalScore.HasValue)
                .Select(s => s.TotalScore!.Value)
                .ToListAsync();

            return scores.Any() ? scores.Average() : 0m;
        }

        public async Task<decimal> GetAverageScoreByCourseAsync(string courseId)
        {
            var scores = await _dbSet
                .Where(s => s.Enrollment.CourseId == courseId && s.TotalScore.HasValue)
                .Select(s => s.TotalScore!.Value)
                .ToListAsync();

            return scores.Any() ? scores.Average() : 0m;
        }

        public async Task<IEnumerable<Score>> GetTopScoresByCourseAsync(string courseId, int topCount = 10)
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Where(s => s.Enrollment.CourseId == courseId && s.TotalScore.HasValue)
                .OrderByDescending(s => s.TotalScore)
                .Take(topCount)
                .ToListAsync();
        }


        public async Task<bool> HasScoreAsync(int enrollmentId)
        {
            return await _dbSet.AnyAsync(s => s.EnrollmentId == enrollmentId);
        }

        public async Task<bool> IsScoreCompleteAsync(int enrollmentId)
        {
            var score = await _dbSet.FirstOrDefaultAsync(s => s.EnrollmentId == enrollmentId);
            return score != null &&
                   score.ProcessScore.HasValue &&
                   score.MidtermScore.HasValue &&
                   score.FinalScore.HasValue;
        }


        public override async Task<IEnumerable<Score>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .Select(s => new Score // ⬅ THÊM SELECT để tránh computed column
                {
                    ScoreId = s.ScoreId,
                    EnrollmentId = s.EnrollmentId,
                    ProcessScore = s.ProcessScore,
                    MidtermScore = s.MidtermScore,
                    FinalScore = s.FinalScore,
                    Enrollment = s.Enrollment
                })
                .OrderByDescending(s => s.Enrollment.Semester.StartDate)
                .ToListAsync(); // ⬅ Line 274
        }


        public override async Task<Score?> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.Enrollment)
                    .ThenInclude(e => e.Semester)
                .FirstOrDefaultAsync(s => s.ScoreId == (int)id);
        }

        public async Task<decimal> GetAverageScoreBySemesterAsync(int semesterId)
        {
            var scores = await _dbSet
                .Where(s => s.Enrollment.SemesterId == semesterId && s.TotalScore.HasValue)
                .Select(s => s.TotalScore!.Value)
                .ToListAsync();

            return scores.Any() ? scores.Average() : 0m;
        }

        public async Task<decimal> GetAverageScoreByTeacherAsync(string teacherId)
        {
            var scores = await _dbSet
                .Where(s => s.Enrollment.TeacherCourse != null &&
                           s.Enrollment.TeacherCourse.TeacherId == teacherId &&
                           s.TotalScore.HasValue)
                .Select(s => s.TotalScore!.Value)
                .ToListAsync();

            return scores.Any() ? scores.Average() : 0m;
        }

        public async Task<int> GetPassingStudentCountByCourseAsync(string courseId, decimal passingScore = 5.0m)
        {
            return await _dbSet
                .CountAsync(s => s.Enrollment.CourseId == courseId &&
                                s.TotalScore.HasValue &&
                                s.TotalScore >= passingScore);
        }

        public async Task<int> GetFailingStudentCountByCourseAsync(string courseId, decimal passingScore = 5.0m)
        {
            return await _dbSet
                .CountAsync(s => s.Enrollment.CourseId == courseId &&
                                s.TotalScore.HasValue &&
                                s.TotalScore < passingScore);
        }

        public async Task<Dictionary<string, int>> GetGradeDistributionByCourseAsync(string courseId)
        {
            var scores = await _dbSet
                .Where(s => s.Enrollment.CourseId == courseId && s.TotalScore.HasValue)
                .Select(s => s.TotalScore!.Value)
                .ToListAsync();

            return new Dictionary<string, int>
            {
                ["A"] = scores.Count(s => s >= 8.5m),
                ["B"] = scores.Count(s => s >= 7.0m && s < 8.5m),
                ["C"] = scores.Count(s => s >= 5.5m && s < 7.0m),
                ["D"] = scores.Count(s => s >= 4.0m && s < 5.5m),
                ["F"] = scores.Count(s => s < 4.0m)
            };
        }
    }
}
