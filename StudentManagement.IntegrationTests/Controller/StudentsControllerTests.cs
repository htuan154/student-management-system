using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentManagementSystem.Controllers;
using StudentManagementSystem.DTOs.Student;
using StudentManagementSystem.Services.Interfaces;
using Xunit;

namespace StudentManagement.IntegrationTests.Controllers
{
    public class StudentsControllerTests
    {
        private readonly Mock<IStudentService> _mockService;
        private readonly StudentsController _controller;

        public StudentsControllerTests()
        {
            _mockService = new Mock<IStudentService>();
            _controller = new StudentsController(_mockService.Object);
        }

        [Fact]
        public async Task GetAllStudents_ShouldReturnOk()
        {
            var students = new List<StudentResponseDto> { new StudentResponseDto { StudentId = "S001", FullName = "John" } };
            _mockService.Setup(s => s.GetAllStudentsAsync()).ReturnsAsync(students);

            var result = await _controller.GetAllStudents();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            ok.Value.Should().BeEquivalentTo(students);
        }

        [Fact]
        public async Task GetStudent_ShouldReturnOk_IfFound()
        {
            var dto = new StudentResponseDto { StudentId = "S001" };
            _mockService.Setup(s => s.GetStudentByIdAsync("S001")).ReturnsAsync(dto);

            var result = await _controller.GetStudent("S001");
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            ok.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetStudent_ShouldReturnNotFound_IfNotFound()
        {
            _mockService.Setup(s => s.GetStudentByIdAsync("S999")).ReturnsAsync((StudentResponseDto)null!);

            var result = await _controller.GetStudent("S999");
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateStudent_ShouldReturnCreated()
        {
            var dto = new CreateStudentDto { FullName = "John" };
            var response = new StudentResponseDto { StudentId = "S001", FullName = "John" };
            _mockService.Setup(s => s.CreateStudentAsync(dto)).ReturnsAsync(response);

            var result = await _controller.CreateStudent(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            created.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task UpdateStudent_ShouldReturnOk_IfFound()
        {
            var dto = new UpdateStudentDto { FullName = "Updated Name" };
            var response = new StudentResponseDto { StudentId = "S001", FullName = "Updated Name" };
            _mockService.Setup(s => s.UpdateStudentAsync("S001", dto)).ReturnsAsync(response);

            var result = await _controller.UpdateStudent("S001", dto);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            ok.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task UpdateStudent_ShouldReturnNotFound_IfNotFound()
        {
            var dto = new UpdateStudentDto { FullName = "Unknown" };
            _mockService.Setup(s => s.UpdateStudentAsync("S999", dto)).ReturnsAsync((StudentResponseDto)null!);

            var result = await _controller.UpdateStudent("S999", dto);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteStudent_ShouldReturnNoContent_IfSuccess()
        {
            _mockService.Setup(s => s.DeleteStudentAsync("S001")).ReturnsAsync(true);

            var result = await _controller.DeleteStudent("S001");
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteStudent_ShouldReturnNotFound_IfFail()
        {
            _mockService.Setup(s => s.DeleteStudentAsync("S999")).ReturnsAsync(false);

            var result = await _controller.DeleteStudent("S999");
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetStudentsByClass_ShouldReturnOk()
        {
            var data = new List<StudentResponseDto> { new StudentResponseDto { StudentId = "S001" } };
            _mockService.Setup(s => s.GetStudentsByClassIdAsync("C001")).ReturnsAsync(data);

            var result = await _controller.GetStudentsByClass("C001");
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            ok.Value.Should().BeEquivalentTo(data);
        }

        [Fact]
        public async Task SearchStudents_ShouldReturnBadRequest_IfTermEmpty()
        {
            var result = await _controller.SearchStudents("");
            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            bad.Value.Should().Be("Search term is required");
        }

        [Fact]
        public async Task GetPagedStudents_ShouldReturnOk()
        {
            var mockData = new List<StudentResponseDto> { new StudentResponseDto { StudentId = "S001" } };
            _mockService.Setup(s => s.GetPagedStudentsAsync(1, 10, null)).ReturnsAsync((mockData, 1));

            var result = await _controller.GetPagedStudents(1, 10);
            var ok = Assert.IsType<OkObjectResult>(result);
            dynamic val = ok.Value!;
            Assert.Equal(1, val.TotalCount);
        }

        [Fact]
        public async Task CheckEmailExists_ShouldReturnOk()
        {
            _mockService.Setup(s => s.IsEmailExistsAsync("a@b.com", null)).ReturnsAsync(true);

            var result = await _controller.CheckEmailExists("a@b.com", null);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            ok.Value.Should().Be(true);
        }

        [Fact]
        public async Task CheckStudentIdExists_ShouldReturnOk()
        {
            _mockService.Setup(s => s.IsStudentIdExistsAsync("S001")).ReturnsAsync(false);

            var result = await _controller.CheckStudentIdExists("S001");
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            ok.Value.Should().Be(false);
        }
    }
}
