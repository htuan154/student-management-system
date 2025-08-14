using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Schedule;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(
            IScheduleRepository scheduleRepository,
            ICacheService cacheService,
            ILogger<ScheduleService> logger)
        {
            _scheduleRepository = scheduleRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<ScheduleDto>> GetAllAsync()
        {
            var schedules = await _scheduleRepository.GetAllAsync();
            return schedules.Select(MapToDto);
        }

        public async Task<ScheduleDto?> GetByIdAsync(int id)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            return schedule == null ? null : MapToDto(schedule);
        }

        public async Task<IEnumerable<ScheduleDto>> GetSchedulesByTeacherCourseIdAsync(int teacherCourseId)
        {
            var schedules = await _scheduleRepository.GetSchedulesByTeacherCourseIdAsync(teacherCourseId);
            return schedules.Select(MapToDto);
        }

        public async Task<IEnumerable<ScheduleDto>> GetSchedulesByTeacherIdAsync(string teacherId)
        {
            var schedules = await _scheduleRepository.GetSchedulesByTeacherIdAsync(teacherId);
            return schedules.Select(MapToDto);
        }

        public async Task<IEnumerable<ScheduleDto>> GetSchedulesByCourseIdAsync(string courseId)
        {
            var schedules = await _scheduleRepository.GetSchedulesByCourseIdAsync(courseId);
            return schedules.Select(MapToDto);
        }

        public async Task<IEnumerable<ScheduleDto>> GetSchedulesByDayAsync(int dayOfWeek)
        {
            var schedules = await _scheduleRepository.GetSchedulesByDayAsync(dayOfWeek);
            return schedules.Select(MapToDto);
        }

        public async Task<IEnumerable<ScheduleDto>> GetSchedulesByRoomAsync(string roomNumber)
        {
            var schedules = await _scheduleRepository.GetSchedulesByRoomAsync(roomNumber);
            return schedules.Select(MapToDto);
        }
        public async Task<IEnumerable<ScheduleDto>> GetStudentScheduleAsync(string studentId, int? semesterId = null)
        {
            var schedules = await _scheduleRepository.GetStudentSchedulesAsync(studentId, semesterId);

            // chặn trùng tuyệt đối theo khóa chính:
            schedules = schedules.GroupBy(s => s.ScheduleId).Select(g => g.First());

            return schedules
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .Select(MapToDto)
                .ToList();
        }

        public Task<IEnumerable<ScheduleDto>> GetMyScheduleAsync(string userId, int? semesterId = null)
            => GetStudentScheduleAsync(userId, semesterId);

        public async Task<IEnumerable<ScheduleDto>> GetStudentDailyScheduleAsync(string studentId, int dayOfWeek, int? semesterId = null)
        {
            var schedules = await _scheduleRepository.GetStudentSchedulesByDayAsync(studentId, dayOfWeek, semesterId);
            schedules = schedules.GroupBy(s => s.ScheduleId).Select(g => g.First());
            return schedules
                .OrderBy(s => s.StartTime)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<ScheduleDto> CreateAsync(CreateScheduleDto createDto)
        {
            // Validation
            var timeConflict = await _scheduleRepository.IsTimeSlotConflictAsync(
                createDto.TeacherCourseId, createDto.DayOfWeek, createDto.StartTime, createDto.EndTime);

            if (timeConflict)
            {
                throw new InvalidOperationException("Time slot conflict detected for this teacher course.");
            }

            if (!string.IsNullOrEmpty(createDto.RoomNumber))
            {
                var roomConflict = await _scheduleRepository.IsRoomConflictAsync(
                    createDto.RoomNumber, createDto.DayOfWeek, createDto.StartTime, createDto.EndTime);

                if (roomConflict)
                {
                    throw new InvalidOperationException("Room conflict detected for this time slot.");
                }
            }

            var schedule = new Schedule
            {
                TeacherCourseId = createDto.TeacherCourseId,
                DayOfWeek = createDto.DayOfWeek,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                RoomNumber = createDto.RoomNumber,
                Location = createDto.Location
            };

            var createdSchedule = await _scheduleRepository.AddAsync(schedule);
            _logger.LogInformation("Created schedule with ID {ScheduleId}.", createdSchedule.ScheduleId);

            return MapToDto(createdSchedule);
        }

        public async Task<ScheduleDto?> UpdateAsync(int id, UpdateScheduleDto updateDto)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null) return null;

            // Validation
            var timeConflict = await _scheduleRepository.IsTimeSlotConflictAsync(
                updateDto.TeacherCourseId, updateDto.DayOfWeek, updateDto.StartTime, updateDto.EndTime, id);

            if (timeConflict)
            {
                throw new InvalidOperationException("Time slot conflict detected for this teacher course.");
            }

            if (!string.IsNullOrEmpty(updateDto.RoomNumber))
            {
                var roomConflict = await _scheduleRepository.IsRoomConflictAsync(
                    updateDto.RoomNumber, updateDto.DayOfWeek, updateDto.StartTime, updateDto.EndTime, id);

                if (roomConflict)
                {
                    throw new InvalidOperationException("Room conflict detected for this time slot.");
                }
            }

            schedule.TeacherCourseId = updateDto.TeacherCourseId;
            schedule.DayOfWeek = updateDto.DayOfWeek;
            schedule.StartTime = updateDto.StartTime;
            schedule.EndTime = updateDto.EndTime;
            schedule.RoomNumber = updateDto.RoomNumber;
            schedule.Location = updateDto.Location;

            await _scheduleRepository.UpdateAsync(schedule);
            _logger.LogInformation("Updated schedule with ID {ScheduleId}.", id);

            return MapToDto(schedule);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _scheduleRepository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Deleted schedule with ID {ScheduleId}.", id);
            }
            return result;
        }

        public async Task<(IEnumerable<ScheduleDto> Schedules, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (schedules, totalCount) = await _scheduleRepository.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return (schedules.Select(MapToDto), totalCount);
        }

        public async Task<bool> IsTimeSlotConflictAsync(int teacherCourseId, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeScheduleId = null)
        {
            return await _scheduleRepository.IsTimeSlotConflictAsync(teacherCourseId, dayOfWeek, startTime, endTime, excludeScheduleId);
        }

        public async Task<bool> IsRoomConflictAsync(string roomNumber, int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeScheduleId = null)
        {
            return await _scheduleRepository.IsRoomConflictAsync(roomNumber, dayOfWeek, startTime, endTime, excludeScheduleId);
        }

        private static ScheduleDto MapToDto(Schedule schedule)
        {
            return new ScheduleDto
            {
                ScheduleId = schedule.ScheduleId,
                TeacherCourseId = schedule.TeacherCourseId,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                RoomNumber = schedule.RoomNumber,
                Location = schedule.Location,

                // Navigation properties
                TeacherName = schedule.TeacherCourse?.Teacher?.FullName,
                CourseName = schedule.TeacherCourse?.Course?.CourseName,
                CourseId = schedule.TeacherCourse?.CourseId,
                SemesterName = schedule.TeacherCourse?.Semester?.SemesterName,
                AcademicYear = schedule.TeacherCourse?.Semester?.AcademicYear
            };
        }

        // ✅ THÊM - Additional methods
        public async Task<IEnumerable<ScheduleDto>> GetWeeklyScheduleAsync(string? teacherId = null, string? courseId = null)
        {
            // ✅ SỬA - Sử dụng repository methods thay vì query in-memory
            IEnumerable<Schedule> schedules;

            if (!string.IsNullOrEmpty(teacherId))
            {
                schedules = await _scheduleRepository.GetSchedulesByTeacherIdAsync(teacherId);
            }
            else if (!string.IsNullOrEmpty(courseId))
            {
                schedules = await _scheduleRepository.GetSchedulesByCourseIdAsync(courseId);
            }
            else
            {
                schedules = await _scheduleRepository.GetAllAsync();
            }


            if (!string.IsNullOrEmpty(teacherId) && !string.IsNullOrEmpty(courseId))
            {
                schedules = schedules.Where(s => s.TeacherCourse?.CourseId == courseId);
            }

            return schedules
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<IEnumerable<ScheduleDto>> GetDailyScheduleAsync(int dayOfWeek, string? teacherId = null)
        {

            IEnumerable<Schedule> schedules;

            if (!string.IsNullOrEmpty(teacherId))
            {
                var teacherSchedules = await _scheduleRepository.GetSchedulesByTeacherIdAsync(teacherId);
                schedules = teacherSchedules.Where(s => s.DayOfWeek == dayOfWeek);
            }
            else
            {
                schedules = await _scheduleRepository.GetSchedulesByDayAsync(dayOfWeek);
            }

            return schedules
                .OrderBy(s => s.StartTime)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<IEnumerable<ScheduleDto>> GetRoomScheduleAsync(string roomNumber, int? dayOfWeek = null)
        {
            var schedules = await _scheduleRepository.GetSchedulesByRoomAsync(roomNumber);

            if (dayOfWeek.HasValue)
            {
                schedules = schedules.Where(s => s.DayOfWeek == dayOfWeek.Value);
            }

            return schedules
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .Select(MapToDto)
                .ToList();
        }
    }
}
