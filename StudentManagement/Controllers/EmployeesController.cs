using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs.Employee;
using StudentManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeResponseDto>> GetEmployee(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeResponseDto>> CreateEmployee(CreateEmployeeDto createEmployeeDto)
        {
            try
            {
                var employee = await _employeeService.CreateEmployeeAsync(createEmployeeDto);
                return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, employee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EmployeeResponseDto>> UpdateEmployee(string id, UpdateEmployeeDto updateEmployeeDto)
        {
            try
            {
                var employee = await _employeeService.UpdateEmployeeAsync(id, updateEmployeeDto);
                if (employee == null)
                    return NotFound();

                return Ok(employee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var result = await _employeeService.DeleteEmployeeAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("department/{department}")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployeesByDepartment(string department)
        {
            var employees = await _employeeService.GetEmployeesByDepartmentAsync(department);
            return Ok(employees);
        }

        [HttpGet("position/{position}")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployeesByPosition(string position)
        {
            var employees = await _employeeService.GetEmployeesByPositionAsync(position);
            return Ok(employees);
        }

        [HttpGet("salary-range")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployeesBySalaryRange(
            [FromQuery] decimal? minSalary = null,
            [FromQuery] decimal? maxSalary = null)
        {
            var employees = await _employeeService.GetEmployeesBySalaryRangeAsync(minSalary, maxSalary);
            return Ok(employees);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> SearchEmployees([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return BadRequest("Search term is required");

            var employees = await _employeeService.SearchEmployeesAsync(searchTerm);
            return Ok(employees);
        }

        [HttpGet("paged")]
        public async Task<ActionResult> GetPagedEmployees(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number and page size must be greater than 0");

            var (employees, totalCount) = await _employeeService.GetPagedEmployeesAsync(pageNumber, pageSize, searchTerm);

            var result = new
            {
                Employees = employees,
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
            var departments = await _employeeService.GetDistinctDepartmentsAsync();
            return Ok(departments);
        }

        [HttpGet("positions")]
        public async Task<ActionResult<IEnumerable<string>>> GetDistinctPositions()
        {
            var positions = await _employeeService.GetDistinctPositionsAsync();
            return Ok(positions);
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email, [FromQuery] string? excludeEmployeeId = null)
        {
            var exists = await _employeeService.IsEmailExistsAsync(email, excludeEmployeeId);
            return Ok(exists);
        }

        [HttpGet("check-employeeid")]
        public async Task<ActionResult<bool>> CheckEmployeeIdExists([FromQuery] string employeeId)
        {
            var exists = await _employeeService.IsEmployeeIdExistsAsync(employeeId);
            return Ok(exists);
        }
    }
}
