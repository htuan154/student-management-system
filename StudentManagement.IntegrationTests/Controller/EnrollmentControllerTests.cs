using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentManagementSystem.Controllers;
using StudentManagementSystem.Dtos.Enrollment;
using StudentManagementSystem.Services.Interfaces;
using Xunit;

namespace StudentManagement.IntegrationTests.Controller
{
    public class EnrollmentControllerTests
    {
        private readonly Mock<IEnrollmentService> _mockService;
        private readonly EnrollmentController _controller;

        public EnrollmentControllerTests()
        {
            _mockService = new Mock<IEnrollmentService>();
            _controller = new EnrollmentController(_mockService.Object);
        }

        [Fact]
        public async Task GetById_Should_Return_Ok_If_Found()
        {
            var dto = new EnrollmentDto { EnrollmentId = 1, StudentId = "S001", CourseId = "C001" };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_If_NotExist()
        {
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((EnrollmentDto)null!);

            var result = await _controller.GetById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetByStudentId_Should_Return_Ok()
        {
            var dtos = new List<EnrollmentDto> { new EnrollmentDto { EnrollmentId = 1, StudentId = "S001" } };
            _mockService.Setup(s => s.GetByStudentIdAsync("S001")).ReturnsAsync(dtos);

            var result = await _controller.GetByStudentId("S001");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().BeEquivalentTo(dtos);
        }

        [Fact]
        public async Task Create_Should_Return_Ok_If_Success()
        {
            var dto = new EnrollmentCreateDto { StudentId = "S001", CourseId = "C001" };
            _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(true);

            var result = await _controller.Create(dto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Create_Should_Return_BadRequest_If_Fail()
        {
            var dto = new EnrollmentCreateDto { StudentId = "S001", CourseId = "C001" };
            _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(false);

            var result = await _controller.Create(dto);

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
