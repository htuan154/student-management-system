// Controllers/TeachersController.cs
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs.Teacher;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeacherResponseDto>>> GetAllTeachers()
        {
            var teachers = await _teacherService.GetAllTeachersAsync();
            return Ok(teachers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeacherResponseDto>> GetTeacher(string id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            if (teacher == null)
                return NotFound();

            return Ok(teacher);
        }

        [HttpGet("{id}/details")]
        public async Task<ActionResult<TeacherDetailResponseDto>> GetTeacherDetails(string id)
        {
            var teacher = await _teacherService.GetTeacherDetailByIdAsync(id);
            if (teacher == null)
                return NotFound();

            return Ok(teacher);
        }

        [HttpPost]
        public async Task<ActionResult<TeacherResponseDto>> CreateTeacher(CreateTeacherDto createTeacherDto)
        {
            try
            {
                var teacher = await _teacherService.CreateTeacherAsync(createTeacherDto);
                return CreatedAtAction(nameof(GetTeacher), new { id = teacher.TeacherId }, teacher);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TeacherResponseDto>> UpdateTeacher(string id, UpdateTeacherDto updateTeacherDto)
        {
            try
            {
                var teacher = await _teacherService.UpdateTeacherAsync(id, updateTeacherDto);
                if (teacher == null)
                    return NotFound();

                return Ok(teacher);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(string id)
        {
            var result = await _teacherService.DeleteTeacherAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("department/{department}")]
        public async Task<ActionResult<IEnumerable<TeacherResponseDto>>> GetTeachersByDepartment(string department)
        {
            var teachers = await _teacherService.GetTeachersByDepartmentAsync(department);
            return Ok(teachers);
        }

        [HttpGet("degree/{degree}")]
        public async Task<ActionResult<IEnumerable<TeacherResponseDto>>> GetTeachersByDegree(string degree)
        {
            var teachers = await _teacherService.GetTeachersByDegreeAsync(degree);
            return Ok(teachers);
        }

        [HttpGet("with-courses")]
        public async Task<ActionResult<IEnumerable<TeacherResponseDto>>> GetTeachersWithActiveCourses()
        {
            var teachers = await _teacherService.GetTeachersWithActiveCoursesAsync();
            return Ok(teachers);
        }

        [HttpGet("salary-range")]
        public async Task<ActionResult<IEnumerable<TeacherResponseDto>>> GetTeachersBySalaryRange(
            [FromQuery] decimal? minSalary = null,
            [FromQuery] decimal? maxSalary = null)
        {
            var teachers = await _teacherService.GetTeachersBySalaryRangeAsync(minSalary, maxSalary);
            return Ok(teachers);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TeacherResponseDto>>> SearchTeachers([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return BadRequest("Search term is required");

            var teachers = await _teacherService.SearchTeachersAsync(searchTerm);
            return Ok(teachers);
        }

        [HttpGet("paged")]
        public async Task<ActionResult> GetPagedTeachers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number and page size must be greater than 0");

            var (teachers, totalCount) = await _teacherService.GetPagedTeachersAsync(pageNumber, pageSize, searchTerm);

            var result = new
            {
                Teachers = teachers,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }

        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<string>>> GetDistinctDepartments()
        {
            var departments = await _teacherService.GetDistinctDepartmentsAsync();
            return Ok(departments);
        }

        [HttpGet("degrees")]
        public async Task<ActionResult<IEnumerable<string>>> GetDistinctDegrees()
        {
            var degrees = await _teacherService.GetDistinctDegreesAsync();
            return Ok(degrees);
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email, [FromQuery] string? excludeTeacherId = null)
        {
            var exists = await _teacherService.IsEmailExistsAsync(email, excludeTeacherId);
            return Ok(exists);
        }

        [HttpGet("check-teacherid")]
        public async Task<ActionResult<bool>> CheckTeacherIdExists([FromQuery] string teacherId)
        {
            var exists = await _teacherService.IsTeacherIdExistsAsync(teacherId);
            return Ok(exists);
        }
    }
}
