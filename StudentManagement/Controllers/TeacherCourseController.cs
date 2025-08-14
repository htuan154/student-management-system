using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs.TeacherCourse;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherCourseController : ControllerBase
    {
        private readonly ITeacherCourseService _teacherCourseService;
        private readonly ILogger<TeacherCourseController> _logger;

        public TeacherCourseController(ITeacherCourseService teacherCourseService, ILogger<TeacherCourseController> logger)
        {
            _teacherCourseService = teacherCourseService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var teacherCourse = await _teacherCourseService.GetByIdAsync(id);
            if (teacherCourse == null) return NotFound();
            return Ok(teacherCourse);
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacherId(string teacherId)
        {
            var teacherCourses = await _teacherCourseService.GetByTeacherIdAsync(teacherId);
            return Ok(teacherCourses);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourseId(string courseId)
        {
            var teacherCourses = await _teacherCourseService.GetByCourseIdAsync(courseId);
            return Ok(teacherCourses);
        }

        [HttpGet("semester/{semesterId}")]
        public async Task<IActionResult> GetBySemesterId(int semesterId)
        {
            var teacherCourses = await _teacherCourseService.GetBySemesterIdAsync(semesterId);
            return Ok(teacherCourses);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TeacherCourseCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exists = await _teacherCourseService.IsTeacherCourseExistsAsync(dto.TeacherId, dto.CourseId, dto.SemesterId);
            if (exists) return BadRequest("Teacher đã được phân công cho course này trong semester này.");

            var result = await _teacherCourseService.CreateAsync(dto);
            if (!result) return BadRequest("Không thể tạo phân công.");

            return Ok(new { Message = "Phân công thành công." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TeacherCourseUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.TeacherCourseId) return BadRequest("ID không khớp.");

            var exists = await _teacherCourseService.IsTeacherCourseExistsAsync(dto.TeacherId, dto.CourseId, dto.SemesterId, id);
            if (exists) return BadRequest("Teacher đã được phân công cho course này trong semester này.");

            var result = await _teacherCourseService.UpdateAsync(dto);
            if (!result) return NotFound();

            return Ok(new { Message = "Cập nhật phân công thành công." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _teacherCourseService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { Message = "Xóa phân công thành công." });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return BadRequest("Search term is required.");

            var teacherCourses = await _teacherCourseService.SearchAsync(searchTerm);
            return Ok(teacherCourses);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number và page size phải lớn hơn 0.");

            var (teacherCourses, totalCount) = await _teacherCourseService.GetPagedAsync(pageNumber, pageSize, searchTerm);

            return Ok(new
            {
                Data = teacherCourses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        [HttpGet("workload/teacher/{teacherId}/semester/{semesterId}")]
        public async Task<IActionResult> GetTeacherWorkload(string teacherId, int semesterId)
        {
            var workload = await _teacherCourseService.GetTeacherWorkloadAsync(teacherId, semesterId);
            return Ok(new { TeacherId = teacherId, SemesterId = semesterId, CourseCount = workload });
        }

        [HttpGet("check-exists")]
        public async Task<IActionResult> CheckExists(
            [FromQuery] string teacherId,
            [FromQuery] string courseId,
            [FromQuery] int semesterId,
            [FromQuery] int? excludeId = null)
        {
            if (string.IsNullOrEmpty(teacherId) || string.IsNullOrEmpty(courseId) || semesterId <= 0)
                return BadRequest("TeacherId, CourseId và SemesterId là bắt buộc.");

            var exists = await _teacherCourseService.IsTeacherCourseExistsAsync(teacherId, courseId, semesterId, excludeId);
            return Ok(new { Exists = exists });
        }
    }
}
