using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Services;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Services.Interfaces;
using StudentManagementSystem.Models;
using StudentManagementSystem.Dtos.Course;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Tests.Services
{
    public class CourseServiceTests
    {
        // --- Dependencies được giả lập (Mocks) ---
        private readonly Mock<ICourseRepository> _mockCourseRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<CourseService>> _mockLogger;

        // --- Đối tượng đang được kiểm thử (System Under Test - SUT) ---
        private readonly ICourseService _courseService;

        public CourseServiceTests()
        {
            _mockCourseRepository = new Mock<ICourseRepository>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<CourseService>>();

            _courseService = new CourseService(
                _mockCourseRepository.Object,
                _mockCacheService.Object,
                _mockLogger.Object
            );
        }

        // -----------------------------------------------------------------------------
        #region GetByIdAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task GetByIdAsync_KhiCoTrongCache_TraVeMonHocTuCache()
        {
            // Arrange
            var courseId = "CS101";
            var cachedCourse = new CourseDto { CourseId = courseId, CourseName = "Introduction to Programming" };
            var cacheKey = $"course:{courseId}";

            _mockCacheService.Setup(c => c.GetDataAsync<CourseDto>(cacheKey)).ReturnsAsync(cachedCourse);

            // Act
            var result = await _courseService.GetByIdAsync(courseId);

            // Assert
            result.Should().BeEquivalentTo(cachedCourse);
            // Quan trọng: Xác minh rằng repository không được gọi đến vì đã có cache
            _mockCourseRepository.Verify(r => r.GetCourseWithEnrollmentsAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_KhiKhongCoTrongCacheVaCoTrongDb_TraVeMonHocVaLuuVaoCache()
        {
            // Arrange
            var courseId = "CS101";
            var courseFromDb = new Course { CourseId = courseId, CourseName = "Introduction to Programming", Enrollments = new List<Enrollment>() };
            var cacheKey = $"course:{courseId}";

            _mockCacheService.Setup(c => c.GetDataAsync<CourseDto>(cacheKey)).ReturnsAsync((CourseDto)null);
            _mockCourseRepository.Setup(r => r.GetCourseWithEnrollmentsAsync(courseId)).ReturnsAsync(courseFromDb);

            // Act
            var result = await _courseService.GetByIdAsync(courseId);

            // Assert
            result.Should().NotBeNull();
            result.CourseId.Should().Be(courseId);

            // Xác minh rằng phương thức SetDataAsync đã được gọi để lưu kết quả vào cache
            _mockCacheService.Verify(c => c.SetDataAsync(cacheKey, It.Is<CourseDto>(dto => dto.CourseId == courseId), It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_KhiKhongTimThay_TraVeNull()
        {
            // Arrange
            var courseId = "NotFound";
            var cacheKey = $"course:{courseId}";

            _mockCacheService.Setup(c => c.GetDataAsync<CourseDto>(cacheKey)).ReturnsAsync((CourseDto)null);
            _mockCourseRepository.Setup(r => r.GetCourseWithEnrollmentsAsync(courseId)).ReturnsAsync((Course)null);

            // Act
            var result = await _courseService.GetByIdAsync(courseId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region CreateAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task CreateAsync_KhiIdChuaTonTai_TaoThanhCongVaTraVeTrue()
        {
            // Arrange
            var dto = new CourseCreateDto { CourseId = "MATH202", CourseName = "Calculus II" };
            _mockCourseRepository.Setup(r => r.IsCourseIdExistsAsync(dto.CourseId)).ReturnsAsync(false);

            // Act
            var result = await _courseService.CreateAsync(dto);

            // Assert
            result.Should().BeTrue();
            // Xác minh AddAsync được gọi đúng 1 lần
            _mockCourseRepository.Verify(r => r.AddAsync(It.Is<Course>(c => c.CourseId == dto.CourseId)), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_KhiIdDaTonTai_KhongTaoVaTraVeFalse()
        {
            // Arrange
            var dto = new CourseCreateDto { CourseId = "CS101", CourseName = "Duplicate Course" };
            _mockCourseRepository.Setup(r => r.IsCourseIdExistsAsync(dto.CourseId)).ReturnsAsync(true);

            // Act
            var result = await _courseService.CreateAsync(dto);

            // Assert
            result.Should().BeFalse();
            // Xác minh không có hành động thêm vào DB nào được thực hiện
            _mockCourseRepository.Verify(r => r.AddAsync(It.IsAny<Course>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region UpdateAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task UpdateAsync_KhiTimThay_CapNhatVaXoaCache()
        {
            // Arrange
            var dto = new CourseUpdateDto { CourseId = "PHY101", CourseName = "Physics I - Updated" };
            var existingCourse = new Course { CourseId = "PHY101", CourseName = "Physics I" };
            var cacheKey = $"course:{dto.CourseId}";

            _mockCourseRepository.Setup(r => r.GetByIdAsync(dto.CourseId)).ReturnsAsync(existingCourse);

            // Act
            var result = await _courseService.UpdateAsync(dto);

            // Assert
            result.Should().BeTrue();
            _mockCourseRepository.Verify(r => r.UpdateAsync(It.Is<Course>(c => c.CourseName == "Physics I - Updated")), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync(cacheKey), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_KhiKhongTimThay_TraVeFalse()
        {
            // Arrange
            var dto = new CourseUpdateDto { CourseId = "NotFound" };
            _mockCourseRepository.Setup(r => r.GetByIdAsync(dto.CourseId)).ReturnsAsync((Course)null);

            // Act
            var result = await _courseService.UpdateAsync(dto);

            // Assert
            result.Should().BeFalse();
            _mockCourseRepository.Verify(r => r.UpdateAsync(It.IsAny<Course>()), Times.Never);
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
            var courseId = "CHEM101";
            var existingCourse = new Course { CourseId = courseId };
            var cacheKey = $"course:{courseId}";

            _mockCourseRepository.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(existingCourse);

            // Act
            var result = await _courseService.DeleteAsync(courseId);

            // Assert
            result.Should().BeTrue();
            _mockCourseRepository.Verify(r => r.DeleteAsync(existingCourse), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync(cacheKey), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_KhiKhongTimThay_TraVeFalse()
        {
            // Arrange
            var courseId = "NotFound";
            _mockCourseRepository.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync((Course)null);

            // Act
            var result = await _courseService.DeleteAsync(courseId);

            // Assert
            result.Should().BeFalse();
            _mockCourseRepository.Verify(r => r.DeleteAsync(It.IsAny<Course>()), Times.Never);
        }

        #endregion
    }
}
