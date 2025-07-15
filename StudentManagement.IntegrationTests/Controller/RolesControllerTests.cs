using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentManagementSystem.Controllers;
using StudentManagementSystem.Dtos.Role;
using StudentManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace StudentManagement.IntegrationTests.Controller
{
    public class RolesControllerTests
    {
        private readonly Mock<IRoleService> _mockRoleService;
        private readonly RoleController _controller;

        public RolesControllerTests()
        {
            _mockRoleService = new Mock<IRoleService>();
            _controller = new RoleController(_mockRoleService.Object);
        }

        // Test for GetAll method
        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfRoles()
        {
            // Arrange
            var roles = new List<RoleDto>
            {
                new RoleDto { RoleId = "1", RoleName = "Admin" },
                new RoleDto { RoleId = "2", RoleName = "User" }
            };
            _mockRoleService.Setup(service => service.GetAllAsync()).ReturnsAsync(roles);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<RoleDto>>(okResult.Value);
            returnValue.Should().HaveCount(2);
        }

        // Test for GetById method when role exists
        [Fact]
        public async Task GetById_ReturnsOkResult_WhenRoleExists()
        {
            // Arrange
            var roleId = "1";
            var role = new RoleDto { RoleId = roleId, RoleName = "Admin" };
            _mockRoleService.Setup(service => service.GetByIdAsync(roleId)).ReturnsAsync(role);

            // Act
            var result = await _controller.GetById(roleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<RoleDto>(okResult.Value);
            returnValue.RoleId.Should().Be(roleId);
        }

        // Test for GetById method when role does not exist
        [Fact]
        public async Task GetById_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = "99";
            _mockRoleService.Setup(service => service.GetByIdAsync(roleId)).ReturnsAsync((RoleDto?)null);

            // Act
            var result = await _controller.GetById(roleId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // Test for Create method with valid model state
        [Fact]
        public async Task Create_ReturnsOkResult_WhenModelStateIsValid()
        {
            // Arrange
            var createDto = new RoleCreateDto { RoleId = "3", RoleName = "Guest" };
            _mockRoleService.Setup(service => service.CreateAsync(createDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        // Test for Create method with invalid model state
        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("RoleName", "Required");
            var createDto = new RoleCreateDto { RoleId = "3" }; // Missing RoleName

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Test for Create method when role already exists
        [Fact]
        public async Task Create_ReturnsConflict_WhenRoleAlreadyExists()
        {
            // Arrange
            var createDto = new RoleCreateDto { RoleId = "1", RoleName = "Admin" };
            _mockRoleService.Setup(service => service.CreateAsync(createDto)).ReturnsAsync(false);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            conflictResult.Value.Should().Be("RoleId already exists.");
        }

        // Test for Update method when successful
        [Fact]
        public async Task Update_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var roleId = "1";
            var updateDto = new RoleUpdateDto { RoleId = roleId, RoleName = "Super Admin" };
            _mockRoleService.Setup(service => service.UpdateAsync(updateDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.Update(roleId, updateDto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        // Test for Update method when ID in URL and body do not match
        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var roleId = "1";
            var updateDto = new RoleUpdateDto { RoleId = "2", RoleName = "Super Admin" };

            // Act
            var result = await _controller.Update(roleId, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            badRequestResult.Value.Should().Be("RoleId mismatch.");
        }

        // Test for Update method when role does not exist
        [Fact]
        public async Task Update_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = "99";
            var updateDto = new RoleUpdateDto { RoleId = roleId, RoleName = "NonExistent" };
            _mockRoleService.Setup(service => service.UpdateAsync(updateDto)).ReturnsAsync(false);

            // Act
            var result = await _controller.Update(roleId, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // Test for Delete method when successful
        [Fact]
        public async Task Delete_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var roleId = "1";
            _mockRoleService.Setup(service => service.DeleteAsync(roleId)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(roleId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        // Test for Delete method when role does not exist
        [Fact]
        public async Task Delete_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = "99";
            _mockRoleService.Setup(service => service.DeleteAsync(roleId)).ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(roleId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // Test for Search method
        [Fact]
        public async Task Search_ReturnsOkResult_WithMatchingRoles()
        {
            // Arrange
            var searchTerm = "Admin";
            var roles = new List<RoleDto> { new RoleDto { RoleId = "1", RoleName = "Admin" } };
            _mockRoleService.Setup(service => service.SearchRolesAsync(searchTerm)).ReturnsAsync(roles);

            // Act
            var result = await _controller.Search(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<RoleDto>>(okResult.Value);
            returnValue.Should().ContainSingle(r => r.RoleName == "Admin");
        }

        // Test for GetPaged method
        [Fact]
        public async Task GetPaged_ReturnsOkResult_WithPagedRoles()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 1;
            var roles = new List<RoleDto> { new RoleDto { RoleId = "1", RoleName = "Admin" } };
            var totalCount = 2;
            _mockRoleService.Setup(s => s.GetPagedRolesAsync(pageNumber, pageSize, null)).ReturnsAsync((roles, totalCount));

            // Act
            var result = await _controller.GetPaged(pageNumber, pageSize, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().NotBeNull();
            // Using reflection to check properties of anonymous type
            var value = okResult.Value;
            var rolesProperty = value.GetType().GetProperty("roles");
            var totalCountProperty = value.GetType().GetProperty("totalCount");

            rolesProperty.Should().NotBeNull();
            totalCountProperty.Should().NotBeNull();

            var returnedRoles = rolesProperty.GetValue(value) as IEnumerable<RoleDto>;
            var returnedTotalCount = Convert.ToInt32(totalCountProperty.GetValue(value));


            returnedRoles.Should().HaveCount(1);
            returnedTotalCount.Should().Be(totalCount);
        }
    }
}
