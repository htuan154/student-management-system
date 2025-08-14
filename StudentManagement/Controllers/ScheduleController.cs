using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs.Schedule;
using StudentManagementSystem.Services.Interfaces;
using System.Security.Claims;
namespace StudentManagementSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(IScheduleService scheduleService, ILogger<ScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var schedules = await _scheduleService.GetAllAsync();
            return Ok(schedules);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null) return NotFound();
            return Ok(schedule);
        }

        [HttpGet("teacher-course/{teacherCourseId}")]
        public async Task<IActionResult> GetByTeacherCourseId(int teacherCourseId)
        {
            var schedules = await _scheduleService.GetSchedulesByTeacherCourseIdAsync(teacherCourseId);
            return Ok(schedules);
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacherId(string teacherId)
        {
            var schedules = await _scheduleService.GetSchedulesByTeacherIdAsync(teacherId);
            return Ok(schedules);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourseId(string courseId)
        {
            var schedules = await _scheduleService.GetSchedulesByCourseIdAsync(courseId);
            return Ok(schedules);
        }

        [HttpGet("room/{roomNumber}")]
        public async Task<IActionResult> GetByRoom(string roomNumber)
        {
            var schedules = await _scheduleService.GetSchedulesByRoomAsync(roomNumber);
            return Ok(schedules);
        }

        [HttpGet("day/{dayOfWeek:int}")]
        public async Task<IActionResult> GetByDayOfWeek(int dayOfWeek)
        {
            if (dayOfWeek < 2 || dayOfWeek > 8)
                return BadRequest("DayOfWeek phải từ 2 (Thứ 2) đến 8 (Chủ nhật).");

            var schedules = await _scheduleService.GetSchedulesByDayAsync(dayOfWeek);
            return Ok(schedules);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateScheduleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _scheduleService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.ScheduleId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateScheduleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);


            dto.ScheduleId = id;

            try
            {
                var result = await _scheduleService.UpdateAsync(id, dto);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _scheduleService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { Message = "Xóa lịch học thành công." });
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number và page size phải lớn hơn 0.");

            var (schedules, totalCount) = await _scheduleService.GetPagedAsync(pageNumber, pageSize, searchTerm);

            return Ok(new
            {
                Data = schedules,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        [HttpGet("check-time-conflict")]
        public async Task<IActionResult> CheckTimeConflict(
            [FromQuery] int teacherCourseId,
            [FromQuery] int dayOfWeek,
            [FromQuery] TimeSpan startTime,
            [FromQuery] TimeSpan endTime,
            [FromQuery] int? excludeScheduleId = null)
        {
            var hasConflict = await _scheduleService.IsTimeSlotConflictAsync(
                teacherCourseId, dayOfWeek, startTime, endTime, excludeScheduleId);

            return Ok(new { HasConflict = hasConflict });
        }

        [HttpGet("check-room-conflict")]
        public async Task<IActionResult> CheckRoomConflict(
            [FromQuery] string roomNumber,
            [FromQuery] int dayOfWeek,
            [FromQuery] TimeSpan startTime,
            [FromQuery] TimeSpan endTime,
            [FromQuery] int? excludeScheduleId = null)
        {
            var hasConflict = await _scheduleService.IsRoomConflictAsync(
                roomNumber, dayOfWeek, startTime, endTime, excludeScheduleId);

            return Ok(new { HasConflict = hasConflict });
        }

        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklySchedule(
            [FromQuery] string? teacherId = null,
            [FromQuery] string? courseId = null)
        {
            var weeklySchedule = await _scheduleService.GetWeeklyScheduleAsync(teacherId, courseId);
            return Ok(weeklySchedule);
        }

        [HttpGet("daily/{dayOfWeek:int}")]
        public async Task<IActionResult> GetDailySchedule(int dayOfWeek, [FromQuery] string? teacherId = null)
        {
            if (dayOfWeek < 2 || dayOfWeek > 8)
                return BadRequest("DayOfWeek phải từ 2 (Thứ 2) đến 8 (Chủ nhật).");

            var dailySchedule = await _scheduleService.GetDailyScheduleAsync(dayOfWeek, teacherId);
            return Ok(dailySchedule);
        }

        [HttpGet("room-schedule/{roomNumber}")]
        public async Task<IActionResult> GetRoomSchedule(string roomNumber, [FromQuery] int? dayOfWeek = null)
        {
            if (dayOfWeek.HasValue && (dayOfWeek < 2 || dayOfWeek > 8))
                return BadRequest("DayOfWeek phải từ 2 (Thứ 2) đến 8 (Chủ nhật).");

            var roomSchedule = await _scheduleService.GetRoomScheduleAsync(roomNumber, dayOfWeek);
            return Ok(roomSchedule);
        }
        [HttpGet("my")]
            public async Task<IActionResult> GetMySchedule([FromQuery] int? semesterId = null)
            {
                // Ưu tiên claim studentId nếu có
                var studentId = User.FindFirst("studentId")?.Value;

                // Fallback: nếu hệ thống coi UserId chính là StudentId
                if (string.IsNullOrEmpty(studentId))
                    studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(studentId))
                    return Unauthorized();

                var data = await _scheduleService.GetMyScheduleAsync(studentId, semesterId);
                return Ok(data);
            }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("student")]
        public async Task<IActionResult> GetByStudent([FromQuery] string studentId, [FromQuery] int? semesterId = null)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                return BadRequest("studentId is required.");

            var data = await _scheduleService.GetStudentScheduleAsync(studentId, semesterId);
            return Ok(data);
        }

    }
}
