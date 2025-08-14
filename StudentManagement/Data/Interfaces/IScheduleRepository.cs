using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IScheduleRepository : IRepository<Schedule>
    {
        Task<IEnumerable<Schedule>> GetSchedulesByTeacherCourseIdAsync(int teacherCourseId);
        Task<IEnumerable<Schedule>> GetSchedulesByTeacherIdAsync(string teacherId);
        Task<IEnumerable<Schedule>> GetSchedulesByCourseIdAsync(string courseId);
        Task<IEnumerable<Schedule>> GetSchedulesByDayAsync(int dayOfWeek);
        Task<IEnumerable<Schedule>> GetSchedulesByRoomAsync(string roomNumber);
        Task<bool> IsTimeSlotConflictAsync(int teacherCourseId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeScheduleId = null);
        Task<bool> IsRoomConflictAsync(string roomNumber, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeScheduleId = null);
        Task<(IEnumerable<Schedule> Schedules, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<Schedule>> GetSchedulesByTeacherAndCourseAsync(string teacherId, string? courseId = null);
        Task<IEnumerable<Schedule>> GetSchedulesByTeacherAndDayAsync(string teacherId, int dayOfWeek);
        Task<IEnumerable<Schedule>> GetWeeklySchedulesAsync(string? teacherId = null, string? courseId = null);

        Task<IEnumerable<Schedule>> GetStudentSchedulesAsync(string studentId, int? semesterId = null);

        Task<IEnumerable<Schedule>> GetStudentSchedulesByDayAsync(string studentId, int dayOfWeek, int? semesterId = null);
    }
}
