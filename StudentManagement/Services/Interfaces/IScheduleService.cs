using StudentManagementSystem.DTOs.Schedule;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface IScheduleService
    {
        Task<IEnumerable<ScheduleDto>> GetAllAsync();
        Task<ScheduleDto?> GetByIdAsync(int id);
        Task<IEnumerable<ScheduleDto>> GetSchedulesByTeacherCourseIdAsync(int teacherCourseId);
        Task<IEnumerable<ScheduleDto>> GetSchedulesByTeacherIdAsync(string teacherId);
        Task<IEnumerable<ScheduleDto>> GetSchedulesByCourseIdAsync(string courseId);
        Task<IEnumerable<ScheduleDto>> GetSchedulesByDayAsync(int dayOfWeek);
        Task<IEnumerable<ScheduleDto>> GetSchedulesByRoomAsync(string roomNumber);
        Task<ScheduleDto> CreateAsync(CreateScheduleDto createDto);
        Task<ScheduleDto?> UpdateAsync(int id, UpdateScheduleDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<(IEnumerable<ScheduleDto> Schedules, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<bool> IsTimeSlotConflictAsync(int teacherCourseId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeScheduleId = null);
        Task<bool> IsRoomConflictAsync(string roomNumber, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeScheduleId = null);
        Task<IEnumerable<ScheduleDto>> GetWeeklyScheduleAsync(string? teacherId = null, string? courseId = null);
        Task<IEnumerable<ScheduleDto>> GetDailyScheduleAsync(int dayOfWeek, string? teacherId = null);
        Task<IEnumerable<ScheduleDto>> GetRoomScheduleAsync(string roomNumber, int? dayOfWeek = null);
        // Lịch cá nhân theo studentId (tuỳ chọn lọc theo kỳ)
        Task<IEnumerable<ScheduleDto>> GetStudentScheduleAsync(string studentId, int? semesterId = null);

        // Lịch “của tôi” lấy từ userId (controller sẽ lấy từ token và gọi hàm này)
        Task<IEnumerable<ScheduleDto>> GetMyScheduleAsync(string userId, int? semesterId = null);

        // (tuỳ chọn) lịch cá nhân theo ngày
        Task<IEnumerable<ScheduleDto>> GetStudentDailyScheduleAsync(string studentId, int dayOfWeek, int? semesterId = null);
        
    }
}
