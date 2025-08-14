using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Services;
using StudentManagementSystem.Services.Interfaces;
using StudentManagementSystem.DTOs.Auth;
using StudentManagementSystem.DTOs.User;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Generic;

namespace StudentManagementSystem.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // --- Sắp xếp (Arrange) các thành phần phụ thuộc giả lập ---
            _mockUserService = new Mock<IUserService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            // Thiết lập cấu hình JWT giả
            var jwtSettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "CungMotMatKhauSieuDaiSieuBaoMat32Byte"}, // Phải đủ dài cho HMAC-SHA256
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Jwt:ExpiresInMinutes", "15"},
                {"Jwt:RefreshTokenExpiresInDays", "7"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(jwtSettings)
                .Build();

            _mockConfiguration.Setup(c => c[It.Is<string>(s => s.StartsWith("Jwt:"))])
                            .Returns((string key) => configuration[key]);

            // --- Khởi tạo đối tượng cần kiểm thử (System Under Test - SUT) ---
            _authService = new AuthService(
                _mockUserService.Object,
                _mockConfiguration.Object,
                _mockLogger.Object
            );
        }

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnTokens()
        {
            // Sắp xếp (Arrange)
            var loginDto = new LoginDto { Username = "testuser", Password = "password123" };
            var userDto = new UserDto { UserId = "user1", Username = "testuser", RoleName = "Student" };

            _mockUserService.Setup(s => s.GetByUsernameAsync(loginDto.Username)).ReturnsAsync(userDto);
            _mockUserService.Setup(s => s.CheckPasswordAsync(loginDto.Username, loginDto.Password)).ReturnsAsync(true);

            // Hành động (Act)
            var (accessToken, refreshToken) = await _authService.LoginAsync(loginDto);

            // Khẳng định (Assert)
            accessToken.Should().NotBeNullOrEmpty();
            refreshToken.Should().NotBeNullOrEmpty();

            // Xác minh rằng phương thức cập nhật token đã được gọi với các tham số chính xác
            _mockUserService.Verify(s => s.UpdateRefreshTokenAsync(
                userDto.UserId,
                It.IsAny<string>(),
                It.Is<DateTime>(dt => dt > DateTime.UtcNow)),
                Times.Once);

            // Kiểm tra các claim trong token (tùy chọn nhưng nên làm)
            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadJwtToken(accessToken);
            decodedToken.Issuer.Should().Be("TestIssuer");
            decodedToken.Audiences.First().Should().Be("TestAudience");
            decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userDto.UserId);
            decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == userDto.Username);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidUsername_ShouldThrowUnauthorizedAccessException()
        {
            // Sắp xếp (Arrange)
            var loginDto = new LoginDto { Username = "nonexistent", Password = "password" };

            _mockUserService.Setup(s => s.GetByUsernameAsync(loginDto.Username)).ReturnsAsync((UserDto)null);

            // Hành động & Khẳng định (Act & Assert)
            await _authService.Invoking(s => s.LoginAsync(loginDto))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid username or password.");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
        {
            // Sắp xếp (Arrange)
            var loginDto = new LoginDto { Username = "testuser", Password = "wrongpassword" };
            var userDto = new UserDto { UserId = "user1", Username = "testuser", RoleName = "Student" };

            _mockUserService.Setup(s => s.GetByUsernameAsync(loginDto.Username)).ReturnsAsync(userDto);
            _mockUserService.Setup(s => s.CheckPasswordAsync(loginDto.Username, loginDto.Password)).ReturnsAsync(false);

            // Hành động & Khẳng định (Act & Assert)
            await _authService.Invoking(s => s.LoginAsync(loginDto))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid username or password.");
        }

        #endregion

        #region RefreshTokenAsync Tests

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
        {
            // Sắp xếp (Arrange)
            var oldRefreshToken = "validRefreshToken";
            var userDto = new UserDto
            {
                UserId = "user1",
                Username = "testuser",
                RoleName = "Admin"
            };

            _mockUserService.Setup(s => s.GetByRefreshTokenAsync(oldRefreshToken)).ReturnsAsync(userDto);

            // Hành động (Act)
            var (newAccessToken, newRefreshToken) = await _authService.RefreshTokenAsync(oldRefreshToken);

            // Khẳng định (Assert)
            newAccessToken.Should().NotBeNullOrEmpty();
            newRefreshToken.Should().NotBeNullOrEmpty();
            newRefreshToken.Should().NotBe(oldRefreshToken); // Phải là một token mới

            _mockUserService.Verify(s => s.UpdateRefreshTokenAsync(
                userDto.UserId,
                It.Is<string>(token => token != oldRefreshToken), // Đảm bảo token mới được lưu
                It.IsAny<DateTime>()),
                Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInvalidToken_ShouldThrowUnauthorizedAccessException()
        {
            // Sắp xếp (Arrange)
            var invalidRefreshToken = "invalidToken";

            _mockUserService.Setup(s => s.GetByRefreshTokenAsync(invalidRefreshToken)).ReturnsAsync((UserDto)null);

            // Hành động & Khẳng định (Act & Assert)
            await _authService.Invoking(s => s.RefreshTokenAsync(invalidRefreshToken))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid or expired refresh token.");
        }

        [Fact]
        public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowUnauthorizedAccessException()
        {
            // Sắp xếp (Arrange)
            var expiredRefreshToken = "expiredRefreshToken";
            var userDto = new UserDto
            {
                UserId = "user1",
                Username = "testuser",
                RoleName = "Student"
            };

            _mockUserService.Setup(s => s.GetByRefreshTokenAsync(expiredRefreshToken))
                .ReturnsAsync((UserDto)null); // giả lập token không hợp lệ

            // Hành động & Khẳng định (Act & Assert)
            await _authService.Invoking(s => s.RefreshTokenAsync(expiredRefreshToken))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid or expired refresh token.");
        }
        #endregion
    }
}
