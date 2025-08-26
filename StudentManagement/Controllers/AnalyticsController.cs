using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Data.Interfaces;
using System.Threading.Tasks;
using System;

namespace StudentManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IStudentAnalyticsRepository _repo;

        public AnalyticsController(IStudentAnalyticsRepository repo) => _repo = repo;

        [HttpGet("top-students")]
        public async Task<IActionResult> GetTopStudents(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] int? semesterId = null)
        {
            try
            {
                var (items, total) = await _repo.GetTopStudentsAsync(page, size, semesterId);
                return Ok(new { data = items, totalCount = total, page, pageSize = size });
            }
            catch (Exception ex)
            {
                // Khi Dev: trả ProblemDetails để FE thấy lỗi cụ thể
                return Problem(
                    title: "GetTopStudents failed",
                    detail: ex.ToString(),
                    statusCode: 500);
            }
        }
    }
}
