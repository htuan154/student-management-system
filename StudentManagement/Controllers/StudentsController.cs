using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs.Student;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetAllStudents()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(students);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentResponseDto>> GetStudent(string id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
                return NotFound();

            return Ok(student);
        }

        [HttpPost]
        public async Task<ActionResult<StudentResponseDto>> CreateStudent(CreateStudentDto createStudentDto)
        {
            try
            {
                var student = await _studentService.CreateStudentAsync(createStudentDto);
                return CreatedAtAction(nameof(GetStudent), new { id = student.StudentId }, student);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StudentResponseDto>> UpdateStudent(string id, UpdateStudentDto updateStudentDto)
        {
            try
            {
                var student = await _studentService.UpdateStudentAsync(id, updateStudentDto);
                if (student == null)
                    return NotFound();

                return Ok(student);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            var result = await _studentService.DeleteStudentAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("class/{classId}")]
        public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetStudentsByClass(string classId)
        {
            var students = await _studentService.GetStudentsByClassIdAsync(classId);
            return Ok(students);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<StudentResponseDto>>> SearchStudents([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return BadRequest("Search term is required");

            var students = await _studentService.SearchStudentsAsync(searchTerm);
            return Ok(students);
        }

        [HttpGet("paged")]
        public async Task<ActionResult> GetPagedStudents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number and page size must be greater than 0");

            var (students, totalCount) = await _studentService.GetPagedStudentsAsync(pageNumber, pageSize, searchTerm);

            var result = new
            {
                Students = students,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email, [FromQuery] string? excludeStudentId = null)
        {
            var exists = await _studentService.IsEmailExistsAsync(email, excludeStudentId);
            return Ok(exists);
        }

        [HttpGet("check-studentid")]
        public async Task<ActionResult<bool>> CheckStudentIdExists([FromQuery] string studentId)
        {
            var exists = await _studentService.IsStudentIdExistsAsync(studentId);
            return Ok(exists);
        }
    }
}
