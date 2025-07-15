using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentManagementSystem.Controllers;
using StudentManagementSystem.DTOs.Employee;
using StudentManagementSystem.Services.Interfaces;
using Xunit;

namespace StudentManagement.IntegrationTests.Controller
{
    public class EmployeesControllerTests
    {
        private readonly Mock<IEmployeeService> _mockService;
        private readonly EmployeesController _controller;

        public EmployeesControllerTests()
        {
            _mockService = new Mock<IEmployeeService>();
            _controller = new EmployeesController(_mockService.Object);
        }

        [Fact]
        public async Task GetAllEmployees_Should_Return_Ok_With_Data()
        {
            var employees = new List<EmployeeResponseDto> { new EmployeeResponseDto { EmployeeId = "E001", FullName = "Alice" } };
            _mockService.Setup(s => s.GetAllEmployeesAsync()).ReturnsAsync(employees);

            var result = await _controller.GetAllEmployees();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().BeEquivalentTo(employees);
        }

        [Fact]
        public async Task GetEmployee_Should_Return_Ok_If_Found()
        {
            var employee = new EmployeeResponseDto { EmployeeId = "E001", FullName = "Alice" };
            _mockService.Setup(s => s.GetEmployeeByIdAsync("E001")).ReturnsAsync(employee);

            var result = await _controller.GetEmployee("E001");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().BeEquivalentTo(employee);
        }

        [Fact]
        public async Task GetEmployee_Should_Return_NotFound_If_Missing()
        {
            _mockService.Setup(s => s.GetEmployeeByIdAsync("E002")).ReturnsAsync((EmployeeResponseDto)null!);

            var result = await _controller.GetEmployee("E002");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateEmployee_Should_Return_CreatedAtAction()
        {
            var createDto = new CreateEmployeeDto { FullName = "Bob", Email = "bob@example.com" };
            var createdEmployee = new EmployeeResponseDto { EmployeeId = "E003", FullName = "Bob", Email = "bob@example.com" };
            _mockService.Setup(s => s.CreateEmployeeAsync(createDto)).ReturnsAsync(createdEmployee);

            var result = await _controller.CreateEmployee(createDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            createdResult.Value.Should().BeEquivalentTo(createdEmployee);
        }

        [Fact]
        public async Task UpdateEmployee_Should_Return_Ok_If_Success()
        {
            var updateDto = new UpdateEmployeeDto { FullName = "Charlie" };
            var updatedEmployee = new EmployeeResponseDto { EmployeeId = "E004", FullName = "Charlie" };
            _mockService.Setup(s => s.UpdateEmployeeAsync("E004", updateDto)).ReturnsAsync(updatedEmployee);

            var result = await _controller.UpdateEmployee("E004", updateDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().BeEquivalentTo(updatedEmployee);
        }

        [Fact]
        public async Task UpdateEmployee_Should_Return_NotFound_If_Missing()
        {
            var updateDto = new UpdateEmployeeDto { FullName = "NotExist" };
            _mockService.Setup(s => s.UpdateEmployeeAsync("E999", updateDto)).ReturnsAsync((EmployeeResponseDto)null!);

            var result = await _controller.UpdateEmployee("E999", updateDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteEmployee_Should_Return_NoContent_If_Success()
        {
            _mockService.Setup(s => s.DeleteEmployeeAsync("E001")).ReturnsAsync(true);

            var result = await _controller.DeleteEmployee("E001");

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEmployee_Should_Return_NotFound_If_Fail()
        {
            _mockService.Setup(s => s.DeleteEmployeeAsync("E002")).ReturnsAsync(false);

            var result = await _controller.DeleteEmployee("E002");

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
