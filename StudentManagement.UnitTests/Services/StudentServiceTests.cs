using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Services;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Services.Interfaces;
using StudentManagementSystem.Models;
using StudentManagementSystem.DTOs.Student;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Tests.Services
{
    public class StudentServiceTests
    {
        // --- Dependencies được giả lập (Mocks) ---
        private readonly Mock<IStudentRepository> _mockStudentRepository;
        private readonly Mock<IEnrollmentRepository> _mockEnrollmentRepository;
        private readonly Mock<IScoreRepository> _mockScoreRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<StudentService>> _mockLogger;

        // --- Đối tượng đang được kiểm thử (System Under Test - SUT) ---
        private readonly IStudentService _studentService;

        public StudentServiceTests()
        {
            _mockStudentRepository = new Mock<IStudentRepository>();
            _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();
            _mockScoreRepository = new Mock<IScoreRepository>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<StudentService>>();

            _studentService = new StudentService(
                _mockStudentRepository.Object,
                _mockEnrollmentRepository.Object,
                _mockScoreRepository.Object,
                _mockCacheService.Object,
                _mockLogger.Object
            );
        }

        // -----------------------------------------------------------------------------
        #region GetStudentByIdAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task GetStudentByIdAsync_KhiCoTrongCache_TraVeSinhVienTuCache()
        {
            // Arrange
            var studentId = "SV001";
            var cachedStudent = new StudentResponseDto { StudentId = studentId, FullName = "Nguyễn Văn A" };
            var cacheKey = $"student:{studentId}";

            _mockCacheService.Setup(c => c.GetDataAsync<StudentResponseDto>(cacheKey)).ReturnsAsync(cachedStudent);

            // Act
            var result = await _studentService.GetStudentByIdAsync(studentId);

            // Assert
            result.Should().BeEquivalentTo(cachedStudent);
            // Quan trọng: Xác minh rằng repository không được gọi đến vì đã có cache
            _mockStudentRepository.Verify(r => r.GetStudentWithClassAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetStudentByIdAsync_KhiKhongCoTrongCacheVaCoTrongDb_TraVeSinhVienVaLuuVaoCache()
        {
            // Arrange
            var studentId = "SV001";
            var studentFromDb = new Student { StudentId = studentId, FullName = "Nguyễn Văn A", Class = new Class { ClassName = "Lớp A" } };
            var cacheKey = $"student:{studentId}";

            _mockCacheService.Setup(c => c.GetDataAsync<StudentResponseDto>(cacheKey)).ReturnsAsync((StudentResponseDto)null);
            _mockStudentRepository.Setup(r => r.GetStudentWithClassAsync(studentId)).ReturnsAsync(studentFromDb);

            // Act
            var result = await _studentService.GetStudentByIdAsync(studentId);

            // Assert
            result.Should().NotBeNull();
            result.StudentId.Should().Be(studentId);
            result.ClassName.Should().Be("Lớp A");

            // Xác minh rằng phương thức SetDataAsync đã được gọi để lưu kết quả vào cache
            _mockCacheService.Verify(c => c.SetDataAsync(cacheKey, It.IsAny<StudentResponseDto>(), It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Fact]
        public async Task GetStudentByIdAsync_KhiKhongTimThay_TraVeNull()
        {
            // Arrange
            var studentId = "NotFound";
            var cacheKey = $"student:{studentId}";

            _mockCacheService.Setup(c => c.GetDataAsync<StudentResponseDto>(cacheKey)).ReturnsAsync((StudentResponseDto)null);
            _mockStudentRepository.Setup(r => r.GetStudentWithClassAsync(studentId)).ReturnsAsync((Student)null);

            // Act
            var result = await _studentService.GetStudentByIdAsync(studentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region CreateStudentAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task CreateStudentAsync_VoiDuLieuHopLe_TaoVaTraVeSinhVien()
        {
            // Arrange
            var dto = new CreateStudentDto { StudentId = "SV123", FullName = "Trần Thị B", Email = "b.tran@example.com" };
            var createdStudent = new Student { StudentId = dto.StudentId };
            var studentWithClass = new Student { StudentId = dto.StudentId, FullName = dto.FullName, Class = new Class { ClassName = "Lớp B" } };

            _mockStudentRepository.Setup(r => r.IsStudentIdExistsAsync(dto.StudentId)).ReturnsAsync(false);
            _mockStudentRepository.Setup(r =>
                                        r.IsEmailExistsAsync(It.Is<string>(e => e == dto.Email), It.IsAny<string?>()))
                                        .ReturnsAsync(false);

            _mockStudentRepository.Setup(r => r.AddAsync(It.IsAny<Student>())).ReturnsAsync(createdStudent);
            _mockStudentRepository.Setup(r => r.GetStudentWithClassAsync(createdStudent.StudentId)).ReturnsAsync(studentWithClass);

            // Act
            var result = await _studentService.CreateStudentAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.StudentId.Should().Be(dto.StudentId);
            result.ClassName.Should().Be("Lớp B");
            _mockStudentRepository.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync("paged_students:*"), Times.Once);
        }

        [Fact]
        public async Task CreateStudentAsync_KhiEmailDaTonTai_NemRaInvalidOperationException()
        {
            // Arrange
            var dto = new CreateStudentDto { StudentId = "SV123", Email = "exists@example.com" };
            _mockStudentRepository.Setup(r => r.IsStudentIdExistsAsync(dto.StudentId)).ReturnsAsync(false);
            _mockStudentRepository.Setup(r => r.IsEmailExistsAsync(dto.Email, null)).ReturnsAsync(true);

            // Act
            Func<Task> act = () => _studentService.CreateStudentAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Email already exists");
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region UpdateStudentAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task UpdateStudentAsync_KhiSinhVienTonTai_CapNhatThanhCong()
        {
            // Arrange
            var studentId = "SV007";
            var dto = new UpdateStudentDto { FullName = "Lê Văn C", Email = "c.le@example.com" };
            var existingStudent = new Student { StudentId = studentId, FullName = "John Doe" };
            var updatedStudentWithClass = new Student { StudentId = studentId, FullName = dto.FullName, Email = dto.Email };
            var cacheKey = $"student:{studentId}";

            _mockStudentRepository.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(existingStudent);
            _mockStudentRepository.Setup(r => r.IsEmailExistsAsync(dto.Email, studentId)).ReturnsAsync(false);
            _mockStudentRepository.Setup(r => r.GetStudentWithClassAsync(studentId)).ReturnsAsync(updatedStudentWithClass);

            // Act
            var result = await _studentService.UpdateStudentAsync(studentId, dto);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be(dto.FullName);
            _mockStudentRepository.Verify(r => r.UpdateAsync(It.Is<Student>(s => s.StudentId == studentId)), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync(cacheKey), Times.Once);
        }

        [Fact]
        public async Task UpdateStudentAsync_KhiSinhVienKhongTonTai_TraVeNull()
        {
            // Arrange
            var studentId = "NotFound";
            _mockStudentRepository.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync((Student)null);

            // Act
            var result = await _studentService.UpdateStudentAsync(studentId, new UpdateStudentDto());

            // Assert
            result.Should().BeNull();
            _mockStudentRepository.Verify(r => r.UpdateAsync(It.IsAny<Student>()), Times.Never);
        }

        #endregion

        // -----------------------------------------------------------------------------
        #region DeleteStudentAsync Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public async Task DeleteStudentAsync_KhiSinhVienTonTai_XoaThanhCongVaTraVeTrue()
        {
            // Arrange
            var studentId = "SV999";
            var existingStudent = new Student { StudentId = studentId };
            var cacheKey = $"student:{studentId}";
            _mockStudentRepository.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(existingStudent);

            // Act
            var result = await _studentService.DeleteStudentAsync(studentId);

            // Assert
            result.Should().BeTrue();
            _mockStudentRepository.Verify(r => r.DeleteAsync(existingStudent), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync(cacheKey), Times.Once);
        }

        [Fact]
        public async Task DeleteStudentAsync_KhiSinhVienKhongTonTai_TraVeFalse()
        {
            // Arrange
            var studentId = "NotFound";
            _mockStudentRepository.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync((Student)null);

            // Act
            var result = await _studentService.DeleteStudentAsync(studentId);

            // Assert
            result.Should().BeFalse();
            _mockStudentRepository.Verify(r => r.DeleteAsync(It.IsAny<Student>()), Times.Never);
        }

        #endregion
    }
}
