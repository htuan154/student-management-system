using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs.Semester;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SemesterController : ControllerBase
    {
        private readonly ISemesterService _semesterService;
        private readonly ILogger<SemesterController> _logger;

        public SemesterController(ISemesterService semesterService, ILogger<SemesterController> logger)
        {
            _semesterService = semesterService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var semesters = await _semesterService.GetAllAsync();
            return Ok(semesters);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var semester = await _semesterService.GetByIdAsync(id);
            if (semester == null) return NotFound();
            return Ok(semester);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSemesters()
        {
            var semesters = await _semesterService.GetActiveSemstersAsync();
            return Ok(semesters);
        }

        [HttpGet("academic-year/{academicYear}")]
        public async Task<IActionResult> GetByAcademicYear(string academicYear)
        {
            var semesters = await _semesterService.GetSemestersByAcademicYearAsync(academicYear);
            return Ok(semesters);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSemesterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exists = await _semesterService.IsSemesterNameExistsAsync(dto.SemesterName, dto.AcademicYear);
            if (exists) return BadRequest($"Semester '{dto.SemesterName}' năm '{dto.AcademicYear}' đã tồn tại.");

            var result = await _semesterService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.SemesterId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSemesterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exists = await _semesterService.IsSemesterNameExistsAsync(dto.SemesterName, dto.AcademicYear, id);
            if (exists) return BadRequest($"Semester '{dto.SemesterName}' năm '{dto.AcademicYear}' đã tồn tại.");

            var result = await _semesterService.UpdateAsync(id, dto);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _semesterService.DeleteAsync(id);
            if (!result) return BadRequest("Không thể xóa semester này vì đang có dữ liệu liên quan.");
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return BadRequest("Search term is required.");

            // Search dùng GetPagedAsync với searchTerm
            var (semesters, _) = await _semesterService.GetPagedAsync(1, 50, searchTerm);
            return Ok(semesters);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null)
        {
            if (pageNumber < 1 || pageSize < 1) return BadRequest("Page number and size must be greater than 0.");

            var (semesters, totalCount) = await _semesterService.GetPagedAsync(pageNumber, pageSize, searchTerm, isActive);

            return Ok(new
            {
                Data = semesters,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

       
    }
}