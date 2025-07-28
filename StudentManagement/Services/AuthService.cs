using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StudentManagementSystem.Dtos.Auth;
using StudentManagementSystem.Dtos.User;
using StudentManagementSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserService userService, IConfiguration config, ILogger<AuthService> logger)
        {
            _userService = userService;
            _config = config;
            _logger = logger;
        }

        public async Task<(string accessToken, string refreshToken)> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Attempting to log in user: {Username}", dto.Username);
            _logger.LogInformation("Password received: {Password}", dto.Password);

            var user = await _userService.GetByUsernameAsync(dto.Username);

            if (user == null)
            {
                _logger.LogWarning("User not found: {Username}", dto.Username);
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            _logger.LogInformation("User found. Checking password...");

            var passwordValid = await _userService.CheckPasswordAsync(dto.Username, dto.Password);
            _logger.LogInformation("Password verification result: {Result}", passwordValid);

            if (!passwordValid)
            {
                _logger.LogWarning("Password verification failed for user: {Username}", dto.Username);
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiresInDays"]!));
            await _userService.UpdateRefreshTokenAsync(user.UserId, refreshToken, refreshTokenExpiry);

            _logger.LogInformation("User {Username} logged in successfully.", dto.Username);
            return (accessToken, refreshToken);
        }

        public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Attempting to refresh token.");

            var user = await _userService.GetByRefreshTokenAsync(refreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token provided.");
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            var refreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiresInDays"]!));
            await _userService.UpdateRefreshTokenAsync(user.UserId, newRefreshToken, refreshTokenExpiry);

            _logger.LogInformation("Token refreshed successfully for user ID: {UserId}", user.UserId);
            return (newAccessToken, newRefreshToken);
        }

        private string GenerateJwtToken(UserDto user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.RoleName)
            };

            
            if (!string.IsNullOrEmpty(user.StudentId))
            {
                claims.Add(new Claim("studentId", user.StudentId));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiresInMinutes"]!)),
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
