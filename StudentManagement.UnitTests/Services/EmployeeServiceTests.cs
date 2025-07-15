using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Services;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Services.Interfaces;
using StudentManagementSystem.Models;
using StudentManagementSystem.DTOs.Employee;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Tests.Services
{
    public class EmployeeServiceTests
    {
        // --- Dependencies được giả lập (Mocks) ---
        private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<EmployeeService>> _mockLogger;

        // --- Đối tượng đang được kiểm thử (System Under Test - SUT) ---
        private readonly IEmployeeService _employeeService;

        public EmployeeServiceTests()
        {
            _mockEmployeeRepository = new Mock<IEmployeeRepository>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<EmployeeService>>();

            _employeeService = new EmployeeService(
                _mockEmployeeRepository.Object,
                _mockCacheService.Object,
                _mockLogger.Object
            );
        }

        // -----------------------------------------------------------------------------
        #region GetEmployeeByIdAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task GetEmployeeByIdAsync_KhiCoTrongCache_TraVeNhanVienTuCache()
        {
            // Arrange
            var employeeId = "NV001";
            var cachedEmployee = new EmployeeResponseDto { EmployeeId = employeeId, FullName = "Nguyễn Văn A" };
            var cacheKey = $"employee:{employeeId}";

            _mockCacheService.Setup(c => c.GetDataAsync<EmployeeResponseDto>(cacheKey)).ReturnsAsync(cachedEmployee);

            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(employeeId);

            // Assert
            result.Should().BeEquivalentTo(cachedEmployee);
            // Quan trọng: Xác minh rằng repository không được gọi đến vì đã có cache
            _mockEmployeeRepository.Verify(r => r.GetEmployeeWithUsersAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_KhiKhongCoTrongCacheVaCoTrongDb_TraVeNhanVienVaLuuVaoCache()
        {
            // Arrange
            var employeeId = "NV001";
            var employeeFromDb = new Employee { EmployeeId = employeeId, FullName = "Nguyễn Văn A", Users = new List<User>() };
            var cacheKey = $"employee:{employeeId}";

            _mockCacheService.Setup(c => c.GetDataAsync<EmployeeResponseDto>(cacheKey)).ReturnsAsync((EmployeeResponseDto)null);
            _mockEmployeeRepository.Setup(r => r.GetEmployeeWithUsersAsync(employeeId)).ReturnsAsync(employeeFromDb);

            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(employeeId);

            // Assert
            result.Should().NotBeNull();
            result.EmployeeId.Should().Be(employeeId);
            result.UserCount.Should().Be(0);

            // Xác minh rằng phương thức SetDataAsync đã được gọi để lưu kết quả vào cache
            _mockCacheService.Verify(c => c.SetDataAsync(cacheKey, It.IsAny<EmployeeResponseDto>(), It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_KhiKhongTimThay_TraVeNull()
        {
            // Arrange
            var employeeId = "NotFound";
            var cacheKey = $"employee:{employeeId}";

            _mockCacheService.Setup(c => c.GetDataAsync<EmployeeResponseDto>(cacheKey)).ReturnsAsync((EmployeeResponseDto)null);
            _mockEmployeeRepository.Setup(r => r.GetEmployeeWithUsersAsync(employeeId)).ReturnsAsync((Employee)null);

            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(employeeId);

            // Assert
            result.Should().BeNull();
        }
        #endregion

        // -----------------------------------------------------------------------------
        #region CreateEmployeeAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task CreateEmployeeAsync_VoiDuLieuHopLe_TaoVaTraVeNhanVien()
        {
            // Arrange
            var dto = new CreateEmployeeDto { EmployeeId = "NV123", FullName = "Trần Thị B", Email = "b.tran@example.com" };
            var createdEmployee = new Employee { EmployeeId = dto.EmployeeId, FullName = dto.FullName, Email = dto.Email };
            var employeeWithUsers = new Employee { EmployeeId = dto.EmployeeId, FullName = dto.FullName, Email = dto.Email, Users = new List<User>() };

            _mockEmployeeRepository.Setup(r => r.IsEmployeeIdExistsAsync(dto.EmployeeId)).ReturnsAsync(false);
            _mockEmployeeRepository.Setup(r => r.IsEmailExistsAsync(dto.Email , null)).ReturnsAsync(false);
            _mockEmployeeRepository.Setup(r => r.AddAsync(It.IsAny<Employee>())).ReturnsAsync(createdEmployee);
            _mockEmployeeRepository.Setup(r => r.GetEmployeeWithUsersAsync(createdEmployee.EmployeeId)).ReturnsAsync(employeeWithUsers);

            // Act
            var result = await _employeeService.CreateEmployeeAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.EmployeeId.Should().Be(dto.EmployeeId);
            _mockEmployeeRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
            // Xác minh cache list đã bị xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync("employee:distinct_departments"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync("employee:distinct_positions"), Times.Once);
        }

        [Fact]
        public async Task CreateEmployeeAsync_KhiEmailDaTonTai_NemRaInvalidOperationException()
        {
            // Arrange
            var dto = new CreateEmployeeDto { EmployeeId = "NV123", Email = "exists@example.com" };
            _mockEmployeeRepository.Setup(r => r.IsEmployeeIdExistsAsync(dto.EmployeeId)).ReturnsAsync(false);
            _mockEmployeeRepository.Setup(r => r.IsEmailExistsAsync(dto.Email, null)).ReturnsAsync(true);

            // Act
            Func<Task> act = () => _employeeService.CreateEmployeeAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Employee ID or Email already exists.");
            _mockEmployeeRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region UpdateEmployeeAsync Tests
        // -----------------------------------------------------------------------------
        [Fact]
        public async Task UpdateEmployeeAsync_KhiNhanVienTonTai_CapNhatThanhCong()
        {
            // Arrange
            var employeeId = "NV007";
            var dto = new UpdateEmployeeDto { FullName = "James Bond", Email = "j.bond@mi6.gov" };
            var existingEmployee = new Employee { EmployeeId = employeeId, FullName = "John Doe" };
            var updatedEmployeeWithUsers = new Employee { EmployeeId = employeeId, FullName = dto.FullName, Email = dto.Email, Users = new List<User>() };

            _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(existingEmployee);
            _mockEmployeeRepository.Setup(r => r.IsEmailExistsAsync(dto.Email, employeeId)).ReturnsAsync(false);
            _mockEmployeeRepository.Setup(r => r.GetEmployeeWithUsersAsync(employeeId)).ReturnsAsync(updatedEmployeeWithUsers);


            // Act
            var result = await _employeeService.UpdateEmployeeAsync(employeeId, dto);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be(dto.FullName);
            _mockEmployeeRepository.Verify(r => r.UpdateAsync(It.Is<Employee>(e => e.EmployeeId == employeeId)), Times.Once);
            // Xác minh cache của nhân viên cụ thể và cache list đã bị xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync($"employee:{employeeId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync("employee:distinct_departments"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync("employee:distinct_positions"), Times.Once);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_KhiNhanVienKhongTonTai_TraVeNull()
        {
            // Arrange
            var employeeId = "NotFound";
            _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync((Employee)null);

            // Act
            var result = await _employeeService.UpdateEmployeeAsync(employeeId, new UpdateEmployeeDto());

            // Assert
            result.Should().BeNull();
            _mockEmployeeRepository.Verify(r => r.UpdateAsync(It.IsAny<Employee>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region DeleteEmployeeAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task DeleteEmployeeAsync_KhiNhanVienTonTai_XoaThanhCongVaTraVeTrue()
        {
            // Arrange
            var employeeId = "NV999";
            var existingEmployee = new Employee { EmployeeId = employeeId };
            _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(existingEmployee);

            // Act
            var result = await _employeeService.DeleteEmployeeAsync(employeeId);

            // Assert
            result.Should().BeTrue();
            _mockEmployeeRepository.Verify(r => r.DeleteAsync(existingEmployee), Times.Once);
            // Xác minh cache đã được xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync($"employee:{employeeId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync("employee:distinct_departments"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync("employee:distinct_positions"), Times.Once);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_KhiNhanVienKhongTonTai_TraVeFalse()
        {
            // Arrange
            var employeeId = "NotFound";
            _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync((Employee)null);

            // Act
            var result = await _employeeService.DeleteEmployeeAsync(employeeId);

            // Assert
            result.Should().BeFalse();
            _mockEmployeeRepository.Verify(r => r.DeleteAsync(It.IsAny<Employee>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region GetDistinctDepartmentsAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task GetDistinctDepartmentsAsync_KhiCoTrongCache_TraVeDuLieuTuCache()
        {
            // Arrange
            var cacheKey = "employee:distinct_departments";
            var cachedDepartments = new List<string> { "IT", "HR" };
            _mockCacheService.Setup(c => c.GetDataAsync<IEnumerable<string>>(cacheKey)).ReturnsAsync(cachedDepartments);

            // Act
            var result = await _employeeService.GetDistinctDepartmentsAsync();

            // Assert
            result.Should().BeEquivalentTo(cachedDepartments);
            _mockEmployeeRepository.Verify(r => r.GetDistinctDepartmentsAsync(), Times.Never);
        }

        [Fact]
        public async Task GetDistinctDepartmentsAsync_KhiKhongCoTrongCache_LayTuDbVaLuuVaoCache()
        {
            // Arrange
            var cacheKey = "employee:distinct_departments";
            var departmentsFromDb = new List<string> { "IT", "HR", "Marketing" };
            _mockCacheService.Setup(c => c.GetDataAsync<IEnumerable<string>>(cacheKey)).ReturnsAsync((IEnumerable<string>)null);
            _mockEmployeeRepository.Setup(r => r.GetDistinctDepartmentsAsync()).ReturnsAsync(departmentsFromDb);

            // Act
            var result = await _employeeService.GetDistinctDepartmentsAsync();

            // Assert
            result.Should().BeEquivalentTo(departmentsFromDb);
            _mockCacheService.Verify(c =>
                        c.SetDataAsync(
                            cacheKey,
                            It.Is<IEnumerable<string>>(list => list.SequenceEqual(departmentsFromDb)),
                            It.IsAny<DateTimeOffset>()),
                        Times.Once);
        }
        #endregion
    }
}
