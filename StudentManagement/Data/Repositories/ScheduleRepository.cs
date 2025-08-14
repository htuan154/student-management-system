using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class ScheduleRepository : GenericRepository<Schedule>, IScheduleRepository
    {
        public ScheduleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Schedule>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Course)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Semester)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public override async Task<Schedule?> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Course)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Semester)
                .FirstOrDefaultAsync(s => s.ScheduleId == (int)id);
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByTeacherCourseIdAsync(int teacherCourseId)
        {
            return await _dbSet
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Course)
                .Where(s => s.TeacherCourseId == teacherCourseId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByTeacherIdAsync(string teacherId)
        {
            return await _dbSet
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Course)
                .Where(s => s.TeacherCourse.TeacherId == teacherId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByCourseIdAsync(string courseId)
        {
            return await _dbSet
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Where(s => s.TeacherCourse.CourseId == courseId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByDayAsync(int dayOfWeek)
        {
            return await _dbSet
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Course)
                .Where(s => s.DayOfWeek == dayOfWeek)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByRoomAsync(string roomNumber)
        {
            return await _dbSet
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Course)
                .Where(s => s.RoomNumber == roomNumber)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<bool> IsTimeSlotConflictAsync(int teacherCourseId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeScheduleId = null)
        {
            var query = _dbSet.Where(s =>
                s.TeacherCourseId == teacherCourseId &&
                s.DayOfWeek == dayOfWeek &&
                ((s.StartTime <= startTime && s.EndTime > startTime) ||
                 (s.StartTime < endTime && s.EndTime >= endTime) ||
                 (s.StartTime >= startTime && s.EndTime <= endTime)));

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(s => s.ScheduleId != excludeScheduleId.Value);
            }

            return await query.AnyAsync();
        }
        public async Task<IEnumerable<Schedule>> GetStudentSchedulesAsync(string studentId, int? semesterId = null)
        {
            // Lấy các teacherCourseId mà SV đã ghi danh (lọc theo kỳ nếu có)
            var enrolledTeacherCourseIds = _context.Enrollments
                .AsNoTracking()
                .Where(e => e.StudentId == studentId
                            && e.TeacherCourseId != null
                            && (semesterId == null || e.SemesterId == semesterId))
                .Select(e => e.TeacherCourseId!.Value);

            // Lấy các lịch thuộc các teacherCourse đó
            var query = _dbSet
                .Include(s => s.TeacherCourse)!.ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourse)!.ThenInclude(tc => tc.Course)
                .Include(s => s.TeacherCourse)!.ThenInclude(tc => tc.Semester)
                .Where(s => enrolledTeacherCourseIds.Contains(s.TeacherCourseId));

            return await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetStudentSchedulesByDayAsync(string studentId, int dayOfWeek, int? semesterId = null)
        {
            var enrolledTeacherCourseIds = _context.Enrollments
                .AsNoTracking()
                .Where(e => e.StudentId == studentId
                            && e.TeacherCourseId != null
                            && (semesterId == null || e.SemesterId == semesterId))
                .Select(e => e.TeacherCourseId!.Value);

            var query = _dbSet
                .Include(s => s.TeacherCourse)!.ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourse)!.ThenInclude(tc => tc.Course)
                .Include(s => s.TeacherCourse)!.ThenInclude(tc => tc.Semester)
                .Where(s => s.DayOfWeek == dayOfWeek && enrolledTeacherCourseIds.Contains(s.TeacherCourseId));

            return await query
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<bool> IsRoomConflictAsync(string roomNumber, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeScheduleId = null)
        {
            var query = _dbSet.Where(s =>
                s.RoomNumber == roomNumber &&
                s.DayOfWeek == dayOfWeek &&
                ((s.StartTime <= startTime && s.EndTime > startTime) ||
                 (s.StartTime < endTime && s.EndTime >= endTime) ||
                 (s.StartTime >= startTime && s.EndTime <= endTime)));

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(s => s.ScheduleId != excludeScheduleId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<(IEnumerable<Schedule> Schedules, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Course)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.TeacherCourse.Teacher.FullName.Contains(searchTerm) ||
                                        s.TeacherCourse.Course.CourseName.Contains(searchTerm) ||
                                        (s.RoomNumber != null && s.RoomNumber.Contains(searchTerm)) ||
                                        (s.Location != null && s.Location.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var schedules = await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (schedules, totalCount);
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByTeacherAndCourseAsync(string teacherId, string? courseId = null)
        {
            var query = _dbSet
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Course)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Semester)
                .Where(s => s.TeacherCourse != null && s.TeacherCourse.TeacherId == teacherId);

            if (!string.IsNullOrEmpty(courseId))
            {
                query = query.Where(s => s.TeacherCourse.CourseId == courseId);
            }

            return await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByTeacherAndDayAsync(string teacherId, int dayOfWeek)
        {
            return await _dbSet
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Course)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Semester)
                .Where(s => s.TeacherCourse != null &&
                           s.TeacherCourse.TeacherId == teacherId &&
                           s.DayOfWeek == dayOfWeek)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetWeeklySchedulesAsync(string? teacherId = null, string? courseId = null)
        {
            var query = _dbSet
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Teacher)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Course)
                .Include(s => s.TeacherCourse)
                    .ThenInclude(tc => tc.Semester)
                .AsQueryable();

            if (!string.IsNullOrEmpty(teacherId))
            {
                query = query.Where(s => s.TeacherCourse != null && s.TeacherCourse.TeacherId == teacherId);
            }

            if (!string.IsNullOrEmpty(courseId))
            {
                query = query.Where(s => s.TeacherCourse != null && s.TeacherCourse.CourseId == courseId);
            }

            return await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }
    }
}
