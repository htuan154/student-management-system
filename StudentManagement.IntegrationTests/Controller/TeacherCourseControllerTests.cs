using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StudentManagementSystem.Controllers;
using StudentManagementSystem.DTOs.Class;
using StudentManagementSystem.DTOs.Course;
using StudentManagementSystem.DTOs.TeacherCourse;
using StudentManagementSystem.Services.Interfaces;
using Xunit;

namespace StudentManagement.IntegrationTests.Controller
{
    public class TeacherCourseControllerTests
    {
        private readonly Mock<ITeacherCourseService> _mockService;
        private readonly Mock<ILogger<TeacherCourseController>> _mockLogger;
        private readonly TeacherCourseController _controller;

        public TeacherCourseControllerTests()
        {
            _mockService = new Mock<ITeacherCourseService>();
            _mockLogger = new Mock<ILogger<TeacherCourseController>>();
            _controller = new TeacherCourseController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetById_Should_Return_Ok_If_Found()
        {
            var dto = new TeacherCourseDto { TeacherCourseId = 1, TeacherId = "T001" };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_If_NotExist()
        {
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((TeacherCourseDto)null!);

            var result = await _controller.GetById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_Should_Return_Ok_If_Success()
        {
            var createDto = new TeacherCourseCreateDto { TeacherId = "T001", CourseId = "C001" };
            _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(true);

            var result = await _controller.Create(createDto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Create_Should_Return_BadRequest_If_Fail()
        {
            var createDto = new TeacherCourseCreateDto { TeacherId = "T001", CourseId = "C001" };
            _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(false);

            var result = await _controller.Create(createDto);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Delete_Should_Return_Ok_If_Success()
        {
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_Should_Return_NotFound_If_Fail()
        {
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(false);

            var result = await _controller.Delete(1);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
