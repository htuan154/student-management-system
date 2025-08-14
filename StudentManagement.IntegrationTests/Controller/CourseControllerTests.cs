using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentManagementSystem.Controllers;
using StudentManagementSystem.DTOs.Course;
using StudentManagementSystem.Services.Interfaces;
using Xunit;

namespace StudentManagement.IntegrationTests.Controller
{
    public class CourseControllerTests
    {
        private readonly Mock<ICourseService> _mockCourseService;
        private readonly CourseController _controller;

        public CourseControllerTests()
        {
            _mockCourseService = new Mock<ICourseService>();
            _controller = new CourseController(_mockCourseService.Object);
        }

        [Fact]
        public async Task GetAll_Should_Return_Ok_With_Courses()
        {
            var mockCourses = new List<CourseListDto> { new CourseListDto { CourseId = "CS101", CourseName = "Intro to CS" } };
            _mockCourseService.Setup(s => s.GetAllAsync()).ReturnsAsync(mockCourses);

            var result = await _controller.GetAll();

            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(mockCourses);
        }

        [Fact]
        public async Task GetById_Should_Return_Ok_If_Found()
        {
            var id = "CS101";
            var mockCourse = new CourseDto { CourseId = id, CourseName = "Intro to CS" };
            _mockCourseService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(mockCourse);

            var result = await _controller.GetById(id);

            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(mockCourse);
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_If_NotFound()
        {
            _mockCourseService.Setup(s => s.GetByIdAsync("INVALID")).ReturnsAsync((CourseDto)null!);

            var result = await _controller.GetById("INVALID");

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Create_Should_Return_Conflict_If_Exists()
        {
            var dto = new CourseCreateDto { CourseId = "CS101", CourseName = "CS" };
            _mockCourseService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(false);

            var result = await _controller.Create(dto);

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task Delete_Should_Return_Ok_If_Success()
        {
            _mockCourseService.Setup(s => s.DeleteAsync("CS101")).ReturnsAsync(true);

            var result = await _controller.Delete("CS101");

            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Delete_Should_Return_NotFound_If_Fail()
        {
            _mockCourseService.Setup(s => s.DeleteAsync("INVALID")).ReturnsAsync(false);

            var result = await _controller.Delete("INVALID");

            result.Should().BeOfType<NotFoundResult>();
        }
    }

}
