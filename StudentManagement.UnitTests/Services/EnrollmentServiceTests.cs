using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Services;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Services.Interfaces;
using StudentManagementSystem.Models;
using StudentManagementSystem.Dtos.Enrollment;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Tests.Services
{
    public class EnrollmentServiceTests
    {
        // --- Dependencies được giả lập (Mocks) ---
        private readonly Mock<IEnrollmentRepository> _mockEnrollmentRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<EnrollmentService>> _mockLogger;

        // --- Đối tượng đang được kiểm thử (System Under Test - SUT) ---
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentServiceTests()
        {
            _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<EnrollmentService>>();

            _enrollmentService = new EnrollmentService(
                _mockEnrollmentRepository.Object,
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
            var enrollmentId = 1;
            var cachedEnrollment = new EnrollmentDto { EnrollmentId = enrollmentId, StudentId = "SV001" };
            var cacheKey = $"enrollment:{enrollmentId}";

            _mockCacheService.Setup(c => c.GetDataAsync<EnrollmentDto>(cacheKey)).ReturnsAsync(cachedEnrollment);

            // Act
            var result = await _enrollmentService.GetByIdAsync(enrollmentId);

            // Assert
            result.Should().BeEquivalentTo(cachedEnrollment);
            // Quan trọng: Xác minh rằng repository không được gọi đến vì đã có cache
            _mockEnrollmentRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_KhiKhongCoTrongCacheVaCoTrongDb_TraVeDuLieuVaLuuVaoCache()
        {
            // Arrange
            var enrollmentId = 1;
            var enrollmentFromDb = new Enrollment { EnrollmentId = enrollmentId, StudentId = "SV001", CourseId = "CS101" };
            var cacheKey = $"enrollment:{enrollmentId}";

            _mockCacheService.Setup(c => c.GetDataAsync<EnrollmentDto>(cacheKey)).ReturnsAsync((EnrollmentDto)null);
            _mockEnrollmentRepository.Setup(r => r.GetByIdAsync(enrollmentId)).ReturnsAsync(enrollmentFromDb);

            // Act
            var result = await _enrollmentService.GetByIdAsync(enrollmentId);

            // Assert
            result.Should().NotBeNull();
            result.EnrollmentId.Should().Be(enrollmentId);

            // Xác minh rằng phương thức SetDataAsync đã được gọi để lưu kết quả vào cache
            _mockCacheService.Verify(c => c.SetDataAsync(cacheKey, It.Is<EnrollmentDto>(e => e.EnrollmentId == enrollmentId), It.IsAny<DateTimeOffset>()), Times.Once);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region GetByStudentIdAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task GetByStudentIdAsync_KhiCoTrongCache_TraVeDanhSachTuCache()
        {
            // Arrange
            var studentId = "SV007";
            var cacheKey = $"enrollment:student:{studentId}";
            var cachedEnrollments = new List<EnrollmentDto> { new EnrollmentDto { StudentId = studentId } };

            _mockCacheService.Setup(c => c.GetDataAsync<IEnumerable<EnrollmentDto>>(cacheKey)).ReturnsAsync(cachedEnrollments);

            // Act
            var result = await _enrollmentService.GetByStudentIdAsync(studentId);

            // Assert
            result.Should().BeEquivalentTo(cachedEnrollments);
            _mockEnrollmentRepository.Verify(r => r.GetByStudentIdAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region CreateAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task CreateAsync_KhiThanhCong_TraVeTrueVaXoaCachesLienQuan()
        {
            // Arrange
            var dto = new EnrollmentCreateDto { StudentId = "SV001", CourseId = "CS101" };

            // Act
            var result = await _enrollmentService.CreateAsync(dto);

            // Assert
            result.Should().BeTrue();
            // Xác minh AddAsync được gọi đúng 1 lần với bất kỳ đối tượng Enrollment nào
            _mockEnrollmentRepository.Verify(r => r.AddAsync(It.IsAny<Enrollment>()), Times.Once);
            // Xác minh các cache liên quan đã được xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync($"enrollment:student:{dto.StudentId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"enrollment:course:{dto.CourseId}"), Times.Once);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region UpdateAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task UpdateAsync_KhiTimThay_CapNhatVaTraVeTrue()
        {
            // Arrange
            var dto = new EnrollmentUpdateDto { EnrollmentId = 1, StudentId = "SV002", CourseId = "MA102", Status = "Completed" };
            var existingEnrollment = new Enrollment { EnrollmentId = 1, StudentId = "SV001", CourseId = "MA101", Status = "In-Progress" };

            _mockEnrollmentRepository.Setup(r => r.GetByIdAsync(dto.EnrollmentId)).ReturnsAsync(existingEnrollment);

            // Act
            var result = await _enrollmentService.UpdateAsync(dto);

            // Assert
            result.Should().BeTrue();
            _mockEnrollmentRepository.Verify(r => r.UpdateAsync(It.Is<Enrollment>(e => e.Status == "Completed")), Times.Once);

            // Xác minh tất cả các cache liên quan đã được xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync($"enrollment:{dto.EnrollmentId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"enrollment:student:{dto.StudentId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"enrollment:course:{dto.CourseId}"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_KhiKhongTimThay_TraVeFalse()
        {
            // Arrange
            var dto = new EnrollmentUpdateDto { EnrollmentId = 999 };
            _mockEnrollmentRepository.Setup(r => r.GetByIdAsync(dto.EnrollmentId)).ReturnsAsync((Enrollment)null);

            // Act
            var result = await _enrollmentService.UpdateAsync(dto);

            // Assert
            result.Should().BeFalse();
            // Không có hành động cập nhật hay xóa cache nào được thực hiện
            _mockEnrollmentRepository.Verify(r => r.UpdateAsync(It.IsAny<Enrollment>()), Times.Never);
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
            var enrollmentId = 5;
            var existingEnrollment = new Enrollment { EnrollmentId = enrollmentId, StudentId = "SV005", CourseId = "PHY101" };
            _mockEnrollmentRepository.Setup(r => r.GetByIdAsync(enrollmentId)).ReturnsAsync(existingEnrollment);

            // Act
            var result = await _enrollmentService.DeleteAsync(enrollmentId);

            // Assert
            result.Should().BeTrue();
            _mockEnrollmentRepository.Verify(r => r.DeleteAsync(existingEnrollment), Times.Once);

            // Xác minh tất cả các cache liên quan đã được xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync($"enrollment:{enrollmentId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"enrollment:student:{existingEnrollment.StudentId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"enrollment:course:{existingEnrollment.CourseId}"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_KhiKhongTimThay_TraVeFalse()
        {
            // Arrange
            var enrollmentId = 999;
            _mockEnrollmentRepository.Setup(r => r.GetByIdAsync(enrollmentId)).ReturnsAsync((Enrollment)null);

            // Act
            var result = await _enrollmentService.DeleteAsync(enrollmentId);

            // Assert
            result.Should().BeFalse();
            _mockEnrollmentRepository.Verify(r => r.DeleteAsync(It.IsAny<Enrollment>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region SearchAsync Test
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task SearchAsync_KhiDuocGoi_TraVeDanhSachDaMap()
        {
            // Arrange
            var searchTerm = "SV001";
            var enrollmentsFromDb = new List<Enrollment>
            {
                new Enrollment { EnrollmentId = 1, StudentId = "SV001", CourseId = "C1"},
                new Enrollment { EnrollmentId = 2, StudentId = "SV001", CourseId = "C2"}
            };
            _mockEnrollmentRepository.Setup(r => r.SearchAsync(searchTerm)).ReturnsAsync(enrollmentsFromDb);

            // Act
            var result = await _enrollmentService.SearchAsync(searchTerm);

            // Assert
            result.Should().HaveCount(2);
            result.First().StudentId.Should().Be("SV001");
            result.All(dto => dto is EnrollmentDto).Should().BeTrue();
        }

        #endregion
    }
}
