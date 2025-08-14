using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentManagementSystem.Controllers;
using StudentManagementSystem.DTOs.User;
using StudentManagementSystem.Services.Interfaces;
using Xunit;

namespace StudentManagement.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkWithUsers()
        {
            var mockUsers = new List<UserDto> { new UserDto { UserId = "U1", Username = "user1" } };
            _mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync(mockUsers);

            var result = await _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().BeEquivalentTo(mockUsers);
        }

        [Fact]
        public async Task GetById_ShouldReturnOkIfFound()
        {
            var userId = "U1";
            var mockUser = new UserDto { UserId = userId, Username = "user1" };
            _mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(mockUser);

            var result = await _controller.GetById(userId);
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().BeEquivalentTo(mockUser);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFoundIfNotExist()
        {
            _mockUserService.Setup(s => s.GetByIdAsync("invalid")).ReturnsAsync((UserDto)null!);

            var result = await _controller.GetById("invalid");
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetByUsername_ShouldReturnOkIfFound()
        {
            var username = "admin";
            var mockUser = new UserDto { UserId = "U2", Username = username };
            _mockUserService.Setup(s => s.GetByUsernameAsync(username)).ReturnsAsync(mockUser);

            var result = await _controller.GetByUsername(username);
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().BeEquivalentTo(mockUser);
        }

        [Fact]
        public async Task Create_ShouldReturnConflictIfExists()
        {
            var dto = new UserCreateDto { Username = "user1", Email = "user1@example.com" };
            _mockUserService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(false);

            var result = await _controller.Create(dto);
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            conflict.Value.Should().Be("Username or Email already exists.");
        }

        [Fact]
        public async Task Delete_ShouldReturnOkIfSuccess()
        {
            _mockUserService.Setup(s => s.DeleteAsync("U1")).ReturnsAsync(true);

            var result = await _controller.Delete("U1");
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Search_ShouldReturnOkWithResults()
        {
            var searchTerm = "admin";
            var mockResults = new List<UserDto> { new UserDto { UserId = "U3", Username = "admin" } };
            _mockUserService.Setup(s => s.SearchUsersAsync(searchTerm)).ReturnsAsync(mockResults);

            var result = await _controller.Search(searchTerm);
            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().BeEquivalentTo(mockResults);
        }
    }
}
