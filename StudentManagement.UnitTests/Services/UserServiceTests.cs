using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Services;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagement.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _cacheServiceMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger<UserService>>();

            _userService = new UserService(
                _userRepositoryMock.Object,
                _cacheServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CheckPasswordAsync_ShouldReturnTrue_WhenPasswordIsCorrect()
        {
            // Arrange (Sắp đặt)
            var username = "testuser";
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Username = username, PasswordHash = hashedPassword };

            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            // Act (Hành động)
            var result = await _userService.CheckPasswordAsync(username, password);

            // Assert (Kiểm tra)
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckPasswordAsync_ShouldReturnFalse_WhenPasswordIsIncorrect()
        {
            // Arrange
            var username = "testuser";
            var correctPassword = "password123";
            var incorrectPassword = "wrongpassword";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);
            var user = new User { Username = username, PasswordHash = hashedPassword };

            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.CheckPasswordAsync(username, incorrectPassword);

            // Assert
            result.Should().BeFalse();
        }
    }
}
