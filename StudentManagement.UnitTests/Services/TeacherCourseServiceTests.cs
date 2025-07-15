using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Services;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Services.Interfaces;
using StudentManagementSystem.Models;
using StudentManagementSystem.Dtos.TeacherCourse;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Tests.Services
{
    public class TeacherCourseServiceTests
    {
        private readonly Mock<ITeacherCourseRepository> _mockRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<TeacherCourseService>> _mockLogger;

        // --- Đối tượng đang được kiểm thử (System Under Test - SUT) ---
        private readonly ITeacherCourseService _service;

        public TeacherCourseServiceTests()
        {
            _mockRepository = new Mock<ITeacherCourseRepository>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<TeacherCourseService>>();

            _service = new TeacherCourseService(
                _mockRepository.Object,
                _mockCacheService.Object,
                _mockLogger.Object
            );
        }

        // -----------------------------------------------------------------------------
        #region GetByIdAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task GetByIdAsync_KhiCoTrongCache_TraVeDuLieuTuCache()
        {
            // Arrange
            var id = 1;
            var cachedDto = new TeacherCourseDto { TeacherCourseId = id, TeacherId = "GV01", CourseId = "C01" };
            var cacheKey = $"teachercourse:{id}";

            _mockCacheService.Setup(c => c.GetDataAsync<TeacherCourseDto>(cacheKey)).ReturnsAsync(cachedDto);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Should().BeEquivalentTo(cachedDto);
            // Quan trọng: Xác minh rằng repository không được gọi đến vì đã có cache
            _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_KhiKhongCoTrongCacheVaCoTrongDb_TraVeDuLieuVaLuuVaoCache()
        {
            // Arrange
            var id = 1;
            var entityFromDb = new TeacherCourse { TeacherCourseId = id, TeacherId = "GV01", CourseId = "C01" };
            var cacheKey = $"teachercourse:{id}";

            _mockCacheService.Setup(c => c.GetDataAsync<TeacherCourseDto>(cacheKey)).ReturnsAsync((TeacherCourseDto)null);
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entityFromDb);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result.TeacherCourseId.Should().Be(id);

            // Xác minh rằng phương thức SetDataAsync đã được gọi để lưu kết quả vào cache
            _mockCacheService.Verify(c => c.SetDataAsync(cacheKey, It.Is<TeacherCourseDto>(dto => dto.TeacherCourseId == id), It.IsAny<DateTimeOffset>()), Times.Once);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region GetByTeacherIdAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task GetByTeacherIdAsync_KhiCoTrongCache_TraVeDanhSachTuCache()
        {
            // Arrange
            var teacherId = "GV007";
            var cacheKey = $"teachercourse:teacher:{teacherId}";
            var cachedList = new List<TeacherCourseDto> { new TeacherCourseDto { TeacherId = teacherId } };

            _mockCacheService.Setup(c => c.GetDataAsync<IEnumerable<TeacherCourseDto>>(cacheKey)).ReturnsAsync(cachedList);

            // Act
            var result = await _service.GetByTeacherIdAsync(teacherId);

            // Assert
            result.Should().BeEquivalentTo(cachedList);
            _mockRepository.Verify(r => r.GetByTeacherIdAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region CreateAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task CreateAsync_KhiThanhCong_TraVeTrueVaXoaCachesLienQuan()
        {
            // Arrange
            var dto = new TeacherCourseCreateDto { TeacherId = "GV01", CourseId = "C01" };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Should().BeTrue();
            // Xác minh AddAsync được gọi đúng 1 lần với bất kỳ đối tượng TeacherCourse nào
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<TeacherCourse>()), Times.Once);

            // Xác minh các cache liên quan đã được xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teachercourse:teacher:{dto.TeacherId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teachercourse:course:{dto.CourseId}"), Times.Once);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region UpdateAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task UpdateAsync_KhiTimThay_CapNhatVaTraVeTrue()
        {
            // Arrange
            var dto = new TeacherCourseUpdateDto { TeacherCourseId = 1, TeacherId = "GV02", CourseId = "C02", IsActive = false };
            var existingEntity = new TeacherCourse { TeacherCourseId = 1, TeacherId = "GV01", CourseId = "C01", IsActive = true };

            _mockRepository.Setup(r => r.GetByIdAsync(dto.TeacherCourseId)).ReturnsAsync(existingEntity);

            // Act
            var result = await _service.UpdateAsync(dto);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<TeacherCourse>(tc => tc.IsActive == false && tc.TeacherId == "GV02")), Times.Once);

            // Xác minh cache cho các giá trị cũ đã được xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teachercourse:teacher:{existingEntity.TeacherId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teachercourse:course:{existingEntity.CourseId}"), Times.Once);

            // Xác minh cache cho các giá trị mới cũng đã được xóa (và cache của chính ID đó)
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teachercourse:{dto.TeacherCourseId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teachercourse:teacher:{dto.TeacherId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teachercourse:course:{dto.CourseId}"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_KhiKhongTimThay_TraVeFalse()
        {
            // Arrange
            var dto = new TeacherCourseUpdateDto { TeacherCourseId = 999 };
            _mockRepository.Setup(r => r.GetByIdAsync(dto.TeacherCourseId)).ReturnsAsync((TeacherCourse)null);

            // Act
            var result = await _service.UpdateAsync(dto);

            // Assert
            result.Should().BeFalse();
            // Không có hành động cập nhật hay xóa cache nào được thực hiện
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TeacherCourse>()), Times.Never);
            _mockCacheService.Verify(c => c.RemoveDataAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region DeleteAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task DeleteAsync_KhiTimThay_XoaVaTraVeTrue()
        {
            // Arrange
            var id = 5;
            var existingEntity = new TeacherCourse { TeacherCourseId = id, TeacherId = "GV05", CourseId = "C05" };
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingEntity);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(r => r.DeleteAsync(existingEntity), Times.Once);

            // Xác minh tất cả các cache liên quan đã được xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teachercourse:{id}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teachercourse:teacher:{existingEntity.TeacherId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teachercourse:course:{existingEntity.CourseId}"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_KhiKhongTimThay_TraVeFalse()
        {
            // Arrange
            var id = 999;
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((TeacherCourse)null);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<TeacherCourse>()), Times.Never);
        }
        #endregion
    }
}
