using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs.AnnouncementDetail;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementDetailController : ControllerBase
    {
        private readonly IAnnouncementDetailService _announcementDetailService;
        private readonly ILogger<AnnouncementDetailController> _logger;

        public AnnouncementDetailController(IAnnouncementDetailService announcementDetailService, ILogger<AnnouncementDetailController> logger)
        {
            _announcementDetailService = announcementDetailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var details = await _announcementDetailService.GetAllAsync();
            return Ok(details);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var detail = await _announcementDetailService.GetByIdAsync(id);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        [HttpGet("announcement/{announcementId}")]
        public async Task<IActionResult> GetByAnnouncementId(int announcementId)
        {
            var details = await _announcementDetailService.GetDetailsByAnnouncementIdAsync(announcementId);
            return Ok(details);
        }

        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRoleId(string roleId)
        {
            var details = await _announcementDetailService.GetDetailsByRoleIdAsync(roleId);
            return Ok(details);
        }

        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetByClassId(string classId)
        {
            var details = await _announcementDetailService.GetDetailsByClassIdAsync(classId);
            return Ok(details);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourseId(string courseId)
        {
            var details = await _announcementDetailService.GetDetailsByCourseIdAsync(courseId);
            return Ok(details);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            var details = await _announcementDetailService.GetDetailsByUserIdAsync(userId);
            return Ok(details);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncementDetailDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _announcementDetailService.CreateAsync(dto);
            if (result == null) return BadRequest("Không thể tạo announcement detail.");

            return CreatedAtAction(nameof(GetById), new { id = result.AnnouncementDetailId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAnnouncementDetailDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _announcementDetailService.UpdateAsync(id, dto);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _announcementDetailService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { Message = "Xóa announcement detail thành công." });
        }

        [HttpDelete("announcement/{announcementId}")]
        public async Task<IActionResult> DeleteByAnnouncementId(int announcementId)
        {
            var result = await _announcementDetailService.DeleteDetailsByAnnouncementIdAsync(announcementId);
            if (!result) return NotFound();
            return Ok(new { Message = "Xóa tất cả announcement detail của announcement thành công." });
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber < 1 || pageSize < 1) 
                return BadRequest("Page number và page size phải lớn hơn 0.");

            var (details, totalCount) = await _announcementDetailService.GetPagedAsync(pageNumber, pageSize, searchTerm);
            
            return Ok(new
            {
                Data = details,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        [HttpPost("bulk-create/{announcementId}")]
        public async Task<IActionResult> BulkCreate(int announcementId, [FromBody] BulkCreateAnnouncementDetailDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _announcementDetailService.CreateBulkDetailsAsync(announcementId, dto);
            if (!result) return BadRequest("Không thể tạo bulk announcement detail.");

            return Ok(new { Message = "Tạo bulk announcement detail thành công." });
        }

        [HttpGet("user-announcement-details/{userId}")]
        public async Task<IActionResult> GetUserAnnouncementDetails(string userId)
        {
            var details = await _announcementDetailService.GetUserAnnouncementDetailsAsync(userId);
            return Ok(details);
        }
    }
}