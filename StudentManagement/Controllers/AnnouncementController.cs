using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs.Announcement;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ILogger<AnnouncementController> _logger;

        public AnnouncementController(IAnnouncementService announcementService, ILogger<AnnouncementController> logger)
        {
            _announcementService = announcementService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var announcements = await _announcementService.GetAllAsync();
            return Ok(announcements);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var announcement = await _announcementService.GetByIdAsync(id);
            if (announcement == null) return NotFound();
            return Ok(announcement);
        }

        [HttpGet("{id}/with-details")]
        public async Task<IActionResult> GetAnnouncementWithDetails(int id)
        {
            var announcement = await _announcementService.GetAnnouncementWithDetailsAsync(id);
            if (announcement == null) return NotFound();
            return Ok(announcement);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var announcements = await _announcementService.GetAnnouncementsByUserAsync(userId);
            return Ok(announcements);
        }

        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRole(string roleId)
        {
            var announcements = await _announcementService.GetAnnouncementsByRoleAsync(roleId);
            return Ok(announcements);
        }

        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetByClass(string classId)
        {
            var announcements = await _announcementService.GetAnnouncementsByClassAsync(classId);
            return Ok(announcements);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(string courseId)
        {
            var announcements = await _announcementService.GetAnnouncementsByCourseAsync(courseId);
            return Ok(announcements);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveAnnouncements()
        {
            var announcements = await _announcementService.GetActiveAnnouncementsAsync();
            return Ok(announcements);
        }

        [HttpGet("expired")]
        public async Task<IActionResult> GetExpiredAnnouncements()
        {
            var announcements = await _announcementService.GetExpiredAnnouncementsAsync();
            return Ok(announcements);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Lấy createdBy từ User.Identity.Name hoặc truyền cứng nếu chưa có xác thực
            var createdBy = User?.Identity?.Name ?? "system";
            var result = await _announcementService.CreateAsync(dto, createdBy);
            return CreatedAtAction(nameof(GetById), new { id = result.AnnouncementId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAnnouncementDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _announcementService.UpdateAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _announcementService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { Message = "Xóa thông báo thành công." });
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var (announcements, totalCount) = await _announcementService.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return Ok(new
            {
                Data = announcements,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
    }
}