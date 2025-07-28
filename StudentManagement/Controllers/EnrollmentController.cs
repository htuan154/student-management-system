using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Dtos.Enrollment;
using StudentManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace StudentManagementSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var enrollment = await _enrollmentService.GetByIdAsync(id);
            if (enrollment == null) return NotFound();
            return Ok(enrollment);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudentId(string studentId)
        {
            var enrollments = await _enrollmentService.GetByStudentIdAsync(studentId);
            return Ok(enrollments);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourseId(string courseId)
        {
            var enrollments = await _enrollmentService.GetByCourseIdAsync(courseId);
            return Ok(enrollments);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            var enrollments = await _enrollmentService.SearchAsync(term);
            return Ok(enrollments);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? searchTerm = null)
        {
            var (enrollments, totalCount) = await _enrollmentService.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return Ok(new { enrollments, totalCount });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EnrollmentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _enrollmentService.CreateAsync(dto);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EnrollmentUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.EnrollmentId) return BadRequest("EnrollmentId mismatch.");
            var result = await _enrollmentService.UpdateAsync(dto);
            if (!result) return NotFound();
            return Ok();
        }
        [HttpGet("unscored")]
        public async Task<IActionResult> GetUnscored()
        {
            var enrollments = await _enrollmentService.GetUnscoredAsync();
            return Ok(enrollments);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _enrollmentService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
