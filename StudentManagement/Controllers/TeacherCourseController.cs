using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Dtos.TeacherCourse;
using StudentManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherCourseController : ControllerBase
    {
        private readonly ITeacherCourseService _teacherCourseService;

        public TeacherCourseController(ITeacherCourseService teacherCourseService)
        {
            _teacherCourseService = teacherCourseService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tc = await _teacherCourseService.GetByIdAsync(id);
            if (tc == null) return NotFound();
            return Ok(tc);
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacherId(string teacherId)
        {
            var tcs = await _teacherCourseService.GetByTeacherIdAsync(teacherId);
            return Ok(tcs);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourseId(string courseId)
        {
            var tcs = await _teacherCourseService.GetByCourseIdAsync(courseId);
            return Ok(tcs);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            var tcs = await _teacherCourseService.SearchAsync(term);
            return Ok(tcs);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? searchTerm = null)
        {
            var (tcs, totalCount) = await _teacherCourseService.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return Ok(new { tcs, totalCount });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TeacherCourseCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _teacherCourseService.CreateAsync(dto);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TeacherCourseUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.TeacherCourseId) return BadRequest("TeacherCourseId mismatch.");
            var result = await _teacherCourseService.UpdateAsync(dto);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _teacherCourseService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
