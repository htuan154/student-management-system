using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentManagementSystem.Controllers;
using StudentManagementSystem.DTOs.Class;
using StudentManagementSystem.Dtos.Course;
using StudentManagementSystem.Services.Interfaces;
using Xunit;

namespace StudentManagement.IntegrationTests.Controller
{
    public class ClassesControllerTests
    {
        private readonly Mock<IClassService> _mockClassService;
        private readonly ClassesController _controller;

        public ClassesControllerTests()
        {
            _mockClassService = new Mock<IClassService>();
            _controller = new ClassesController(_mockClassService.Object);
        }

        [Fact]
        public async Task GetAllClasses_Should_Return_Ok_With_Data()
        {
            var mockData = new List<ClassResponseDto> { new ClassResponseDto { ClassId = "C001", ClassName = "Class A" } };
            _mockClassService.Setup(s => s.GetAllClassesAsync()).ReturnsAsync(mockData);

            var result = await _controller.GetAllClasses();

            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(mockData);
        }

        [Fact]
        public async Task GetClass_Should_Return_Ok_If_Found()
        {
            var classId = "C001";
            var mockClass = new ClassResponseDto { ClassId = classId, ClassName = "Test" };
            _mockClassService.Setup(s => s.GetClassByIdAsync(classId)).ReturnsAsync(mockClass);

            var result = await _controller.GetClass(classId);

            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(mockClass);
        }

        [Fact]
        public async Task GetClass_Should_Return_NotFound_If_NotFound()
        {
            _mockClassService.Setup(s => s.GetClassByIdAsync("C002")).ReturnsAsync((ClassResponseDto)null!);

            var result = await _controller.GetClass("C002");

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateClass_Should_Return_Created_If_Success()
        {
            var dto = new CreateClassDto { ClassId = "C003", ClassName = "New Class" };
            var response = new ClassResponseDto { ClassId = dto.ClassId, ClassName = dto.ClassName };
            _mockClassService.Setup(s => s.CreateClassAsync(dto)).ReturnsAsync(response);

            var result = await _controller.CreateClass(dto);

            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task DeleteClass_Should_Return_NoContent_If_Success()
        {
            _mockClassService.Setup(s => s.DeleteClassAsync("C001")).ReturnsAsync(true);

            var result = await _controller.DeleteClass("C001");

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteClass_Should_Return_NotFound_If_Fail()
        {
            _mockClassService.Setup(s => s.DeleteClassAsync("C002")).ReturnsAsync(false);

            var result = await _controller.DeleteClass("C002");

            result.Should().BeOfType<NotFoundResult>();
        }
    }

    
}
