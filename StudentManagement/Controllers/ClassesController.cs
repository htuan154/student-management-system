using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs.Class;
using StudentManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ApiController]
    [Route("api/[controller]")]
    public class ClassesController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassesController(IClassService classService)
        {
            _classService = classService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassResponseDto>>> GetAllClasses()
        {
            var classes = await _classService.GetAllClassesAsync();
            return Ok(classes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClassResponseDto>> GetClass(string id)
        {
            var classItem = await _classService.GetClassByIdAsync(id);
            if (classItem == null)
                return NotFound();

            return Ok(classItem);
        }

        [HttpGet("{id}/students")]
        public async Task<ActionResult<ClassWithStudentsDto>> GetClassWithStudents(string id)
        {
            var classItem = await _classService.GetClassWithStudentsAsync(id);
            if (classItem == null)
                return NotFound();

            return Ok(classItem);
        }

        [HttpPost]
        public async Task<ActionResult<ClassResponseDto>> CreateClass(CreateClassDto createClassDto)
        {
            try
            {
                var classItem = await _classService.CreateClassAsync(createClassDto);
                return CreatedAtAction(nameof(GetClass), new { id = classItem.ClassId }, classItem);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ClassResponseDto>> UpdateClass(string id, UpdateClassDto updateClassDto)
        {
            try
            {
                var classItem = await _classService.UpdateClassAsync(id, updateClassDto);
                if (classItem == null)
                    return NotFound();

                return Ok(classItem);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass(string id)
        {
            try
            {
                var result = await _classService.DeleteClassAsync(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<ClassResponseDto>>> GetActiveClasses()
        {
            var classes = await _classService.GetActiveClassesAsync();
            return Ok(classes);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ClassResponseDto>>> SearchClasses([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return BadRequest("Search term is required");

            var classes = await _classService.SearchClassesAsync(searchTerm);
            return Ok(classes);
        }

        [HttpGet("paged")]
        public async Task<ActionResult> GetPagedClasses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number and page size must be greater than 0");

            var (classes, totalCount) = await _classService.GetPagedClassesAsync(pageNumber, pageSize, searchTerm, isActive);

            var result = new
            {
                Classes = classes,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }

        [HttpGet("major/{major}")]
        public async Task<ActionResult<IEnumerable<ClassResponseDto>>> GetClassesByMajor(string major)
        {
            var classes = await _classService.GetClassesByMajorAsync(major);
            return Ok(classes);
        }

        [HttpGet("academic-year/{academicYear}")]
        public async Task<ActionResult<IEnumerable<ClassResponseDto>>> GetClassesByAcademicYear(string academicYear)
        {
            var classes = await _classService.GetClassesByAcademicYearAsync(academicYear);
            return Ok(classes);
        }

        [HttpGet("check-classid")]
        public async Task<ActionResult<bool>> CheckClassIdExists([FromQuery] string classId)
        {
            var exists = await _classService.IsClassIdExistsAsync(classId);
            return Ok(exists);
        }

        [HttpGet("check-classname")]
        public async Task<ActionResult<bool>> CheckClassNameExists([FromQuery] string className, [FromQuery] string? excludeClassId = null)
        {
            var exists = await _classService.IsClassNameExistsAsync(className, excludeClassId);
            return Ok(exists);
        }

        [HttpGet("{id}/can-delete")]
        public async Task<ActionResult<bool>> CanDeleteClass(string id)
        {
            var canDelete = await _classService.CanDeleteClassAsync(id);
            return Ok(canDelete);
        }
    }
}
