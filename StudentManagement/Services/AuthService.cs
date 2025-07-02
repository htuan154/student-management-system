using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudentManagementSystem.Dtos.Auth;
using StudentManagementSystem.Dtos.User;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StudentManagementSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public AuthService(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        public async Task<(string accessToken, string refreshToken)> LoginAsync(LoginDto dto)
        {
            var user = await _userService.GetByUsernameAsync(dto.Username);
            if (user == null || !await _userService.CheckPasswordAsync(dto.Username, dto.Password))
                throw new UnauthorizedAccessException("Invalid username or password.");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            await _userService.UpdateRefreshTokenAsync(user.UserId, refreshToken, DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiresInDays"])));

            return (accessToken, refreshToken);
        }

        public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userService.GetByRefreshTokenAsync(refreshToken);
            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            await _userService.UpdateRefreshTokenAsync(user.UserId, newRefreshToken, DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiresInDays"])));

            return (newAccessToken, newRefreshToken);
        }

        private string GenerateJwtToken(UserDto user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.RoleId)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
