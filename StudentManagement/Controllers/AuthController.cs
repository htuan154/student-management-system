using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs.Auth;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var (accessToken, refreshToken) = await _authService.LoginAsync(dto);
                return Ok(new
                {
                    access_token = accessToken,
                    refresh_token = refreshToken
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            try
            {
                var (accessToken, refreshToken) = await _authService.RefreshTokenAsync(dto.RefreshToken);
                return Ok(new
                {
                    access_token = accessToken,
                    refresh_token = refreshToken
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
