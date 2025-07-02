using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Dtos.Score;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScoreController : ControllerBase
    {
        private readonly IScoreService _scoreService;

        public ScoreController(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var score = await _scoreService.GetByIdAsync(id);
            if (score == null) return NotFound();
            return Ok(score);
        }

        [HttpGet("enrollment/{enrollmentId}")]
        public async Task<IActionResult> GetByEnrollmentId(int enrollmentId)
        {
            var score = await _scoreService.GetByEnrollmentIdAsync(enrollmentId);
            if (score == null) return NotFound();
            return Ok(score);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            var scores = await _scoreService.SearchScoresAsync(term);
            return Ok(scores);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? searchTerm = null)
        {
            var (scores, totalCount) = await _scoreService.GetPagedScoresAsync(pageNumber, pageSize, searchTerm);
            return Ok(new { scores, totalCount });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ScoreCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _scoreService.CreateAsync(dto);
            if (!result) return BadRequest();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ScoreUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.ScoreId) return BadRequest("ScoreId mismatch.");
            var result = await _scoreService.UpdateAsync(dto);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _scoreService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}