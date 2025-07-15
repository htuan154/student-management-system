using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentManagementSystem.Controllers;
using StudentManagementSystem.DTOs.Teacher;
using StudentManagementSystem.Services.Interfaces;
using Xunit;

namespace StudentManagementSystem.Tests.Controllers
{
    public class TeachersControllerTests
    {
        private readonly Mock<ITeacherService> _mockService;
        private readonly TeachersController _controller;

        public TeachersControllerTests()
        {
            _mockService = new Mock<ITeacherService>();
            _controller = new TeachersController(_mockService.Object);
        }

        [Fact]
        public async Task GetAllTeachers_Should_Return_Ok_With_Data()
        {
            var data = new List<TeacherResponseDto> { new TeacherResponseDto { TeacherId = "T001", FullName = "Alice" } };
            _mockService.Setup(s => s.GetAllTeachersAsync()).ReturnsAsync(data);

            var result = await _controller.GetAllTeachers();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().BeEquivalentTo(data);
        }

        [Fact]
        public async Task GetTeacher_Should_Return_Ok_If_Found()
        {
            var teacher = new TeacherResponseDto { TeacherId = "T001", FullName = "Bob" };
            _mockService.Setup(s => s.GetTeacherByIdAsync("T001")).ReturnsAsync(teacher);

            var result = await _controller.GetTeacher("T001");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().BeEquivalentTo(teacher);
        }

        [Fact]
        public async Task GetTeacher_Should_Return_NotFound_If_Missing()
        {
            _mockService.Setup(s => s.GetTeacherByIdAsync("T002")).ReturnsAsync((TeacherResponseDto)null!);

            var result = await _controller.GetTeacher("T002");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateTeacher_Should_Return_Created()
        {
            var dto = new CreateTeacherDto { TeacherId = "T003", FullName = "New Teacher" };
            var created = new TeacherResponseDto { TeacherId = dto.TeacherId, FullName = dto.FullName };
            _mockService.Setup(s => s.CreateTeacherAsync(dto)).ReturnsAsync(created);

            var result = await _controller.CreateTeacher(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            createdResult.Value.Should().BeEquivalentTo(created);
        }

        [Fact]
        public async Task DeleteTeacher_Should_Return_NoContent_If_Success()
        {
            _mockService.Setup(s => s.DeleteTeacherAsync("T004")).ReturnsAsync(true);

            var result = await _controller.DeleteTeacher("T004");

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTeacher_Should_Return_NotFound_If_Fail()
        {
            _mockService.Setup(s => s.DeleteTeacherAsync("T005")).ReturnsAsync(false);

            var result = await _controller.DeleteTeacher("T005");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CheckTeacherIdExists_Should_Return_True_If_Exists()
        {
            _mockService.Setup(s => s.IsTeacherIdExistsAsync("T123")).ReturnsAsync(true);

            var result = await _controller.CheckTeacherIdExists("T123");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True((bool)okResult.Value!);
        }
    }
}
