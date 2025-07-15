using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Services;
using StudentManagementSystem.Services.Interfaces;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;
using StudentManagementSystem.DTOs.Class;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using System.Linq;
using System;

namespace StudentManagementSystem.Tests.Services
{
    public class ClassServiceTests
    {
        // --- Dependencies được giả lập (Mocks) ---
        private readonly Mock<IClassRepository> _mockClassRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<ClassService>> _mockLogger;

        // --- Đối tượng đang được kiểm thử (System Under Test - SUT) ---
        private readonly ClassService _classService;

        public ClassServiceTests()
        {
            // Khởi tạo các mock
            _mockClassRepository = new Mock<IClassRepository>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<ClassService>>(); // Hoặc Mock.Of<ILogger<ClassService>>() cho logger đơn giản

            // Khởi tạo ClassService với các dependency đã được giả lập
            _classService = new ClassService(
                _mockClassRepository.Object,
                _mockCacheService.Object,
                _mockLogger.Object
            );
        }

        #region GetClassByIdAsync Tests

        [Fact]
        public async Task GetClassByIdAsync_WhenCacheHit_ReturnsClassFromCache()
        {
            // Arrange (Sắp xếp)
            var classId = "IT001";
            var cachedClass = new ClassResponseDto { ClassId = classId, ClassName = "Software Engineering" };
            string cacheKey = $"class:{classId}";

            // Giả lập rằng cache service tìm thấy dữ liệu
            _mockCacheService.Setup(c => c.GetDataAsync<ClassResponseDto>(cacheKey))
                            .ReturnsAsync(cachedClass);

            // Act (Hành động)
            var result = await _classService.GetClassByIdAsync(classId);

            // Assert (Khẳng định)
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(cachedClass);

            // Quan trọng: Xác minh rằng repository không được gọi đến vì đã có cache
            _mockClassRepository.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetClassByIdAsync_WhenCacheMissAndDbFound_ReturnsClassAndCachesIt()
        {
            // Arrange
            var classId = "IT001";
            var classFromDb = new Class { ClassId = classId, ClassName = "Software Engineering", IsActive = true };
            string cacheKey = $"class:{classId}";

            // Giả lập cache miss
            _mockCacheService.Setup(c => c.GetDataAsync<ClassResponseDto>(cacheKey))
                             .ReturnsAsync((ClassResponseDto)null);

            // Giả lập repository trả về dữ liệu
            _mockClassRepository.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync(classFromDb);
            _mockClassRepository.Setup(r => r.GetStudentCountInClassAsync(classId)).ReturnsAsync(10);

            // Act
            var result = await _classService.GetClassByIdAsync(classId);

            // Assert
            result.Should().NotBeNull();
            result.ClassId.Should().Be(classId);
            result.StudentCount.Should().Be(10);

            // Xác minh rằng cache đã được thiết lập với dữ liệu mới
            _mockCacheService.Verify(c => c.SetDataAsync(
                cacheKey,
                It.Is<ClassResponseDto>(dto => dto.ClassId == classId),
                It.IsAny<DateTimeOffset>()),
                Times.Once);
        }

        [Fact]
        public async Task GetClassByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var classId = "NONEXISTENT";
            string cacheKey = $"class:{classId}";

            _mockCacheService.Setup(c => c.GetDataAsync<ClassResponseDto>(cacheKey)).ReturnsAsync((ClassResponseDto)null);
            _mockClassRepository.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync((Class)null);

            // Act
            var result = await _classService.GetClassByIdAsync(classId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateClassAsync Tests

        [Fact]
        public async Task CreateClassAsync_WithValidData_CreatesAndReturnsClass()
        {
            // Arrange
            var createDto = new CreateClassDto { ClassId = "CS101", ClassName = "Intro to CS" };
            var classModel = new Class { ClassId = createDto.ClassId, ClassName = createDto.ClassName };

            // Giả lập rằng class ID và name chưa tồn tại
            _mockClassRepository.Setup(r => r.IsClassIdExistsAsync(createDto.ClassId)).ReturnsAsync(false);
            _mockClassRepository.Setup(r => r.IsClassNameExistsAsync(It.Is<string>(name => name == createDto.ClassName),
                                        It.Is<string>(id => id == createDto.ClassId)))
    .ReturnsAsync(false);

            // Giả lập phương thức AddAsync trả về class đã tạo
            _mockClassRepository.Setup(r => r.AddAsync(It.IsAny<Class>())).ReturnsAsync(classModel);

            // Act
            var result = await _classService.CreateClassAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.ClassId.Should().Be(createDto.ClassId);
            result.StudentCount.Should().Be(0); // Lớp mới tạo có 0 sinh viên

            // Xác minh AddAsync đã được gọi
            _mockClassRepository.Verify(r => r.AddAsync(It.Is<Class>(c => c.ClassId == createDto.ClassId)), Times.Once);
        }

        [Fact]
        public async Task CreateClassAsync_WhenClassIdExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var createDto = new CreateClassDto { ClassId = "CS101", ClassName = "Intro to CS" };
            _mockClassRepository.Setup(r => r.IsClassIdExistsAsync(createDto.ClassId)).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _classService.CreateClassAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Class ID or name already exists.");
        }

        #endregion

        #region UpdateClassAsync Tests

        [Fact]
        public async Task UpdateClassAsync_WithValidData_UpdatesAndReturnsClass()
        {
            // Arrange
            var classId = "MA202";
            var updateDto = new UpdateClassDto { ClassName = "Calculus II", Major = "Mathematics" };
            var existingClass = new Class { ClassId = classId, ClassName = "Calculus I" };
            string cacheKey = $"class:{classId}";
            string cacheKeyWithStudents = $"{cacheKey}:students";

            _mockClassRepository.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync(existingClass);
            _mockClassRepository.Setup(r => r.IsClassNameExistsAsync(updateDto.ClassName, classId)).ReturnsAsync(false);
            _mockClassRepository.Setup(r => r.GetStudentCountInClassAsync(classId)).ReturnsAsync(25);

            // Act
            var result = await _classService.UpdateClassAsync(classId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.ClassName.Should().Be("Calculus II");
            result.Major.Should().Be("Mathematics");
            result.StudentCount.Should().Be(25);

            // Xác minh rằng phương thức UpdateAsync và RemoveDataAsync (để xóa cache) đã được gọi
            _mockClassRepository.Verify(r => r.UpdateAsync(It.Is<Class>(c => c.ClassId == classId)), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync(cacheKey), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync(cacheKeyWithStudents), Times.Once);
        }

        [Fact]
        public async Task UpdateClassAsync_WhenClassNotFound_ReturnsNull()
        {
            // Arrange
            var classId = "NONEXISTENT";
            var updateDto = new UpdateClassDto();
            _mockClassRepository.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync((Class)null);

            // Act
            var result = await _classService.UpdateClassAsync(classId, updateDto);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeleteClassAsync Tests

        [Fact]
        public async Task DeleteClassAsync_WhenClassIsEmpty_DeletesAndReturnsTrue()
        {
            // Arrange
            var classId = "PH101";
            var existingClass = new Class { ClassId = classId };
            string cacheKey = $"class:{classId}";
            string cacheKeyWithStudents = $"{cacheKey}:students";

            _mockClassRepository.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync(existingClass);
            // Giả lập rằng lớp có thể xóa (không có sinh viên)
            _mockClassRepository.Setup(r => r.CanDeleteClassAsync(classId)).ReturnsAsync(true);

            // Act
            var result = await _classService.DeleteClassAsync(classId);

            // Assert
            result.Should().BeTrue();
            _mockClassRepository.Verify(r => r.DeleteAsync(existingClass), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync(cacheKey), Times.Once);
            _mockCacheService.Verify(c => c.RemoveDataAsync(cacheKeyWithStudents), Times.Once);
        }

        [Fact]
        public async Task DeleteClassAsync_WhenClassHasStudents_ThrowsInvalidOperationException()
        {
            // Arrange
            var classId = "PH101";
            var existingClass = new Class { ClassId = classId };

            _mockClassRepository.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync(existingClass);
            // Giả lập rằng lớp không thể xóa (có sinh viên)
            _mockClassRepository.Setup(r => r.CanDeleteClassAsync(classId)).ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _classService.DeleteClassAsync(classId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot delete class that has students enrolled.");
        }

        [Fact]
        public async Task DeleteClassAsync_WhenClassNotFound_ReturnsFalse()
        {
            // Arrange
            var classId = "NONEXISTENT";
            _mockClassRepository.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync((Class)null);

            // Act
            var result = await _classService.DeleteClassAsync(classId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetAllClassesAsync (N+1 Problem Demonstration)

        [Fact]
        public async Task GetAllClassesAsync_WhenCalled_ReturnsAllClassesWithStudentCounts()
        {
            // Arrange
            var classesFromDb = new List<Class>
            {
                new Class { ClassId = "A" },
                new Class { ClassId = "B" }
            };

            _mockClassRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(classesFromDb);
            _mockClassRepository.Setup(r => r.GetStudentCountInClassAsync("A")).ReturnsAsync(5);
            _mockClassRepository.Setup(r => r.GetStudentCountInClassAsync("B")).ReturnsAsync(10);

            // Act
            var result = await _classService.GetAllClassesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First(r => r.ClassId == "A").StudentCount.Should().Be(5);
            result.First(r => r.ClassId == "B").StudentCount.Should().Be(10);

            // Xác minh GetStudentCountInClassAsync được gọi cho mỗi lớp
            _mockClassRepository.Verify(r => r.GetStudentCountInClassAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        #endregion
    }
}
