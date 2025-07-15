using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Services;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Services.Interfaces;
using StudentManagementSystem.Models;
using StudentManagementSystem.Dtos.Role;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Tests.Services
{
    public class RoleServiceTests
    {
        // --- Dependencies được giả lập (Mocks) ---
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<RoleService>> _mockLogger;
        private const string AllRolesCacheKey = "roles:all";

        // --- Đối tượng đang được kiểm thử (System Under Test - SUT) ---
        private readonly IRoleService _roleService;

        public RoleServiceTests()
        {
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<RoleService>>();

            _roleService = new RoleService(
                _mockRoleRepository.Object,
                _mockCacheService.Object,
                _mockLogger.Object
            );
        }

        // -----------------------------------------------------------------------------
        #region GetAllAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task GetAllAsync_KhiCoTrongCache_TraVeDanhSachVaiTroTuCache()
        {
            // Arrange
            var cachedRoles = new List<RoleDto>
            {
                new RoleDto { RoleId = "Admin", RoleName = "Administrator" },
                new RoleDto { RoleId = "User", RoleName = "Standard User" }
            };

            _mockCacheService.Setup(c => c.GetDataAsync<IEnumerable<RoleDto>>(AllRolesCacheKey)).ReturnsAsync(cachedRoles);

            // Act
            var result = await _roleService.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(cachedRoles);
            // Quan trọng: Xác minh rằng repository không được gọi đến vì đã có cache
            _mockRoleRepository.Verify(r => r.GetAllAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_KhiKhongCoTrongCache_LayTuDbVaLuuVaoCache()
        {
            // Arrange
            var rolesFromDb = new List<Role>
            {
                new Role { RoleId = "Admin", RoleName = "Administrator" },
                new Role { RoleId = "User", RoleName = "Standard User" }
            };

            _mockCacheService.Setup(c => c.GetDataAsync<IEnumerable<RoleDto>>(AllRolesCacheKey)).ReturnsAsync((IEnumerable<RoleDto>)null);
            _mockRoleRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rolesFromDb);

            // Act
            var result = await _roleService.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().RoleId.Should().Be("Admin");

            // Xác minh rằng phương thức SetDataAsync đã được gọi để lưu kết quả vào cache
            _mockCacheService.Verify(c => c.SetDataAsync(AllRolesCacheKey, It.IsAny<IEnumerable<RoleDto>>(), It.IsAny<DateTimeOffset>()), Times.Once);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region CreateAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task CreateAsync_KhiIdChuaTonTai_TaoThanhCongVaTraVeTrue()
        {
            // Arrange
            var dto = new RoleCreateDto { RoleId = "Guest", RoleName = "Guest User" };
            _mockRoleRepository.Setup(r => r.IsRoleIdExistsAsync(dto.RoleId)).ReturnsAsync(false);

            // Act
            var result = await _roleService.CreateAsync(dto);

            // Assert
            result.Should().BeTrue();
            // Xác minh AddAsync được gọi đúng 1 lần
            _mockRoleRepository.Verify(r => r.AddAsync(It.Is<Role>(r => r.RoleId == dto.RoleId)), Times.Once);
            // Xác minh cache của danh sách tất cả vai trò đã được xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync(AllRolesCacheKey), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_KhiIdDaTonTai_KhongTaoVaTraVeFalse()
        {
            // Arrange
            var dto = new RoleCreateDto { RoleId = "Admin", RoleName = "Duplicate Admin" };
            _mockRoleRepository.Setup(r => r.IsRoleIdExistsAsync(dto.RoleId)).ReturnsAsync(true);

            // Act
            var result = await _roleService.CreateAsync(dto);

            // Assert
            result.Should().BeFalse();
            // Xác minh không có hành động thêm vào DB hay xóa cache nào được thực hiện
            _mockRoleRepository.Verify(r => r.AddAsync(It.IsAny<Role>()), Times.Never);
            _mockCacheService.Verify(c => c.RemoveDataAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region UpdateAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task UpdateAsync_KhiTimThay_CapNhatVaXoaCaches()
        {
            // Arrange
            var dto = new RoleUpdateDto { RoleId = "User", RoleName = "Updated User", Description = "New description" };
            var existingRole = new Role { RoleId = "User", RoleName = "Standard User" };
            var roleCacheKey = $"role:{dto.RoleId}";

            _mockRoleRepository.Setup(r => r.GetByIdAsync(dto.RoleId)).ReturnsAsync(existingRole);

            // Act
            var result = await _roleService.UpdateAsync(dto);

            // Assert
            result.Should().BeTrue();
            _mockRoleRepository.Verify(r => r.UpdateAsync(It.Is<Role>(r => r.RoleName == "Updated User")), Times.Once);

            // Xác minh cả hai cache (cụ thể và danh sách) đều bị xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync(roleCacheKey), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync(AllRolesCacheKey), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_KhiKhongTimThay_TraVeFalse()
        {
            // Arrange
            var dto = new RoleUpdateDto { RoleId = "NotFound" };
            _mockRoleRepository.Setup(r => r.GetByIdAsync(dto.RoleId)).ReturnsAsync((Role)null);

            // Act
            var result = await _roleService.UpdateAsync(dto);

            // Assert
            result.Should().BeFalse();
            _mockRoleRepository.Verify(r => r.UpdateAsync(It.IsAny<Role>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region DeleteAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task DeleteAsync_KhiTimThay_XoaVaTraVeTrue()
        {
            // Arrange
            var roleId = "ToDelete";
            var existingRole = new Role { RoleId = roleId };
            var roleCacheKey = $"role:{roleId}";

            _mockRoleRepository.Setup(r => r.GetByIdAsync(roleId)).ReturnsAsync(existingRole);

            // Act
            var result = await _roleService.DeleteAsync(roleId);

            // Assert
            result.Should().BeTrue();
            _mockRoleRepository.Verify(r => r.DeleteAsync(existingRole), Times.Once);

            // Xác minh cả hai cache đều bị xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync(roleCacheKey), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync(AllRolesCacheKey), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_KhiKhongTimThay_TraVeFalse()
        {
            // Arrange
            var roleId = "NotFound";
            _mockRoleRepository.Setup(r => r.GetByIdAsync(roleId)).ReturnsAsync((Role)null);

            // Act
            var result = await _roleService.DeleteAsync(roleId);

            // Assert
            result.Should().BeFalse();
            _mockRoleRepository.Verify(r => r.DeleteAsync(It.IsAny<Role>()), Times.Never);
        }

        #endregion
    }
}
