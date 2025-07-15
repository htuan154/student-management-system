using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Services;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Services.Interfaces;
using StudentManagementSystem.Models;
using StudentManagementSystem.DTOs.Teacher;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Tests.Services
{
    public class TeacherServiceTests
    {
        // --- Dependencies được giả lập (Mocks) ---
        private readonly Mock<ITeacherRepository> _mockTeacherRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<TeacherService>> _mockLogger;

        // --- Đối tượng đang được kiểm thử (System Under Test - SUT) ---
        private readonly ITeacherService _teacherService;

        public TeacherServiceTests()
        {
            _mockTeacherRepository = new Mock<ITeacherRepository>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<TeacherService>>();

            _teacherService = new TeacherService(
                _mockTeacherRepository.Object,
                _mockCacheService.Object,
                _mockLogger.Object
            );
        }

        // -----------------------------------------------------------------------------
        #region GetTeacherByIdAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task GetTeacherByIdAsync_KhiCoTrongCache_TraVeGiaoVienTuCache()
        {
            // Arrange
            var teacherId = "GV001";
            var cachedTeacher = new TeacherResponseDto { TeacherId = teacherId, FullName = "Nguyễn Thị A" };
            var cacheKey = $"teacher:response:{teacherId}";

            _mockCacheService.Setup(c => c.GetDataAsync<TeacherResponseDto>(cacheKey)).ReturnsAsync(cachedTeacher);

            // Act
            var result = await _teacherService.GetTeacherByIdAsync(teacherId);

            // Assert
            result.Should().BeEquivalentTo(cachedTeacher);
            // Quan trọng: Xác minh rằng repository không được gọi đến vì đã có cache
            _mockTeacherRepository.Verify(r => r.GetTeacherWithUsersAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetTeacherByIdAsync_KhiKhongCoTrongCacheVaCoTrongDb_TraVeGiaoVienVaLuuVaoCache()
        {
            // Arrange
            var teacherId = "GV001";
            var teacherFromDb = new Teacher { TeacherId = teacherId, FullName = "Nguyễn Thị A", Users = new List<User>() };
            var cacheKey = $"teacher:response:{teacherId}";

            _mockCacheService.Setup(c => c.GetDataAsync<TeacherResponseDto>(cacheKey)).ReturnsAsync((TeacherResponseDto)null);
            _mockTeacherRepository.Setup(r => r.GetTeacherWithUsersAsync(teacherId)).ReturnsAsync(teacherFromDb);
            _mockTeacherRepository.Setup(r => r.GetTeacherCourseCountAsync(teacherId)).ReturnsAsync(5);
            _mockTeacherRepository.Setup(r => r.GetTeacherEnrollmentCountAsync(teacherId)).ReturnsAsync(100);

            // Act
            var result = await _teacherService.GetTeacherByIdAsync(teacherId);

            // Assert
            result.Should().NotBeNull();
            result.TeacherId.Should().Be(teacherId);
            result.CourseCount.Should().Be(5);
            result.EnrollmentCount.Should().Be(100);

            // Xác minh rằng phương thức SetDataAsync đã được gọi để lưu kết quả vào cache
            _mockCacheService.Verify(c => c.SetDataAsync(cacheKey, It.Is<TeacherResponseDto>(t => t.TeacherId == teacherId), It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Fact]
        public async Task GetTeacherByIdAsync_KhiKhongTimThay_TraVeNull()
        {
            // Arrange
            var teacherId = "NotFound";
            _mockCacheService.Setup(c => c.GetDataAsync<TeacherResponseDto>(It.IsAny<string>())).ReturnsAsync((TeacherResponseDto)null);
            _mockTeacherRepository.Setup(r => r.GetTeacherWithUsersAsync(teacherId)).ReturnsAsync((Teacher)null);

            // Act
            var result = await _teacherService.GetTeacherByIdAsync(teacherId);


            // Assert
            result.Should().BeNull();
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region CreateTeacherAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task CreateTeacherAsync_VoiDuLieuHopLe_TaoVaTraVeGiaoVien()
        {
            // Arrange
            var dto = new CreateTeacherDto { TeacherId = "GV123", FullName = "Trần Văn B", Email = "b.tran@example.com" };
            var createdTeacher = new Teacher { TeacherId = dto.TeacherId, FullName = dto.FullName, Email = dto.Email };

            _mockTeacherRepository.Setup(r => r.IsTeacherIdExistsAsync(dto.TeacherId)).ReturnsAsync(false);
            _mockTeacherRepository.Setup(r => r.IsEmailExistsAsync(dto.Email, null)).ReturnsAsync(false);
            _mockTeacherRepository.Setup(r => r.AddAsync(It.IsAny<Teacher>())).ReturnsAsync(createdTeacher);
            _mockTeacherRepository.Setup(r => r.GetTeacherCourseCountAsync(dto.TeacherId)).ReturnsAsync(0);
            _mockTeacherRepository.Setup(r => r.GetTeacherEnrollmentCountAsync(dto.TeacherId)).ReturnsAsync(0);


            // Act
            var result = await _teacherService.CreateTeacherAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.TeacherId.Should().Be(dto.TeacherId);
            _mockTeacherRepository.Verify(r => r.AddAsync(It.IsAny<Teacher>()), Times.Once);
            // Xác minh cache list đã bị xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync("teachers:distinct_departments"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync("teachers:distinct_degrees"), Times.Once);
        }

        [Fact]
        public async Task CreateTeacherAsync_KhiEmailDaTonTai_NemRaInvalidOperationException()
        {
            // Arrange
            var dto = new CreateTeacherDto { TeacherId = "GV123", Email = "exists@example.com" };
            _mockTeacherRepository.Setup(r => r.IsTeacherIdExistsAsync(dto.TeacherId)).ReturnsAsync(false);
            _mockTeacherRepository.Setup(r => r.IsEmailExistsAsync(dto.Email, null)).ReturnsAsync(true);

            // Act
            Func<Task> act = () => _teacherService.CreateTeacherAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Teacher ID or Email already exists.");
            _mockTeacherRepository.Verify(r => r.AddAsync(It.IsAny<Teacher>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region UpdateTeacherAsync Tests
        // -----------------------------------------------------------------------------
        [Fact]
        public async Task UpdateTeacherAsync_KhiGiaoVienTonTai_CapNhatThanhCong()
        {
            // Arrange
            var teacherId = "GV007";
            var dto = new UpdateTeacherDto { FullName = "Lê Thị C", Email = "c.le@mi6.gov" };
            var existingTeacher = new Teacher { TeacherId = teacherId, FullName = "John Doe" };

            _mockTeacherRepository.Setup(r => r.GetByIdAsync(teacherId)).ReturnsAsync(existingTeacher);
            _mockTeacherRepository.Setup(r => r.IsEmailExistsAsync(dto.Email, teacherId)).ReturnsAsync(false);

            // Act
            var result = await _teacherService.UpdateTeacherAsync(teacherId, dto);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be(dto.FullName);
            _mockTeacherRepository.Verify(r => r.UpdateAsync(It.Is<Teacher>(t => t.TeacherId == teacherId)), Times.Once);
            // Xác minh các cache liên quan đã bị xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teacher:response:{teacherId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teacher:detail:{teacherId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync("teachers:distinct_departments"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync("teachers:distinct_degrees"), Times.Once);
        }

        [Fact]
        public async Task UpdateTeacherAsync_KhiGiaoVienKhongTonTai_TraVeNull()
        {
            // Arrange
            var teacherId = "NotFound";
            _mockTeacherRepository.Setup(r => r.GetByIdAsync(teacherId)).ReturnsAsync((Teacher)null);

            // Act
            var result = await _teacherService.UpdateTeacherAsync(teacherId, new UpdateTeacherDto());

            // Assert
            result.Should().BeNull();
            _mockTeacherRepository.Verify(r => r.UpdateAsync(It.IsAny<Teacher>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region DeleteTeacherAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task DeleteTeacherAsync_KhiGiaoVienTonTai_XoaThanhCongVaTraVeTrue()
        {
            // Arrange
            var teacherId = "GV999";
            var existingTeacher = new Teacher { TeacherId = teacherId };
            _mockTeacherRepository.Setup(r => r.GetByIdAsync(teacherId)).ReturnsAsync(existingTeacher);

            // Act
            var result = await _teacherService.DeleteTeacherAsync(teacherId);

            // Assert
            result.Should().BeTrue();
            _mockTeacherRepository.Verify(r => r.DeleteAsync(existingTeacher), Times.Once);
            // Xác minh cache đã được xóa
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teacher:response:{teacherId}"), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync($"teacher:detail:{teacherId}"), Times.Once);
        }

        [Fact]
        public async Task DeleteTeacherAsync_KhiGiaoVienKhongTonTai_TraVeFalse()
        {
            // Arrange
            var teacherId = "NotFound";
            _mockTeacherRepository.Setup(r => r.GetByIdAsync(teacherId)).ReturnsAsync((Teacher)null);

            // Act
            var result = await _teacherService.DeleteTeacherAsync(teacherId);

            // Assert
            result.Should().BeFalse();
            _mockTeacherRepository.Verify(r => r.DeleteAsync(It.IsAny<Teacher>()), Times.Never);
        }

        #endregion
    }
}
