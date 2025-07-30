using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs.Course;
using StudentManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace StudentManagementSystem.Controllers
{
    [Authorize()]
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _courseService.GetAllAsync();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null) return NotFound();
            return Ok(course);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _courseService.CreateAsync(dto);
            if (!result) return Conflict("CourseId already exists.");
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CourseUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.CourseId) return BadRequest("CourseId mismatch.");
            var result = await _courseService.UpdateAsync(dto);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _courseService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            var courses = await _courseService.SearchCoursesAsync(term);
            return Ok(courses);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? searchTerm = null, [FromQuery] bool? isActive = null)
        {
            var (courses, totalCount) = await _courseService.GetPagedCoursesAsync(pageNumber, pageSize, searchTerm, isActive);
            return Ok(new { courses, totalCount });
        }

        [HttpGet("department/{department}")]
        public async Task<IActionResult> GetByDepartment(string department)
        {
            var courses = await _courseService.GetCoursesByDepartmentAsync(department);
            return Ok(courses);
        }

        [HttpGet("{id}/enrollment-count")]
        public async Task<IActionResult> GetEnrollmentCount(string id)
        {
            var count = await _courseService.GetEnrollmentCountInCourseAsync(id);
            return Ok(count);
        }
    }
}
