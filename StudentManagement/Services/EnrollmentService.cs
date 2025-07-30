using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Enrollment;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace StudentManagementSystem.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<EnrollmentService> _logger;
        private const string CachePrefix = "enrollment";

        public EnrollmentService(IEnrollmentRepository enrollmentRepository, ICacheService cacheService, ILogger<EnrollmentService> logger)
        {
            _enrollmentRepository = enrollmentRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<EnrollmentDto?> GetByIdAsync(int enrollmentId)
        {
            _logger.LogInformation("Getting enrollment by ID: {EnrollmentId}", enrollmentId);
            var cacheKey = $"{CachePrefix}:{enrollmentId}";

            var cachedData = await _cacheService.GetDataAsync<EnrollmentDto>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Enrollment {EnrollmentId} found in cache.", enrollmentId);
                return cachedData;
            }

            var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
            if (enrollment == null) return null;

            var dto = MapToDto(enrollment);
            await _cacheService.SetDataAsync(cacheKey, dto, System.DateTimeOffset.Now.AddMinutes(5));
            return dto;
        }

        public async Task<IEnumerable<EnrollmentDto>> GetByStudentIdAsync(string studentId)
        {
            _logger.LogInformation("Getting enrollments for Student ID: {StudentId}", studentId);
            var cacheKey = $"{CachePrefix}:student:{studentId}";

            var cachedData = await _cacheService.GetDataAsync<IEnumerable<EnrollmentDto>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Enrollments for Student ID {StudentId} found in cache.", studentId);
                return cachedData;
            }

            var enrollments = await _enrollmentRepository.GetByStudentIdAsync(studentId);
            var dtos = enrollments.Select(MapToDto).ToList();
            await _cacheService.SetDataAsync(cacheKey, dtos, System.DateTimeOffset.Now.AddMinutes(5));
            return dtos;
        }

        public async Task<IEnumerable<EnrollmentDto>> GetByCourseIdAsync(string courseId)
        {
            _logger.LogInformation("Getting enrollments for Course ID: {CourseId}", courseId);
            var cacheKey = $"{CachePrefix}:course:{courseId}";

            var cachedData = await _cacheService.GetDataAsync<IEnumerable<EnrollmentDto>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Enrollments for Course ID {CourseId} found in cache.", courseId);
                return cachedData;
            }

            var enrollments = await _enrollmentRepository.GetByCourseIdAsync(courseId);
            var dtos = enrollments.Select(MapToDto).ToList();
            await _cacheService.SetDataAsync(cacheKey, dtos, System.DateTimeOffset.Now.AddMinutes(5));
            return dtos;
        }

        public async Task<bool> CreateAsync(EnrollmentCreateDto dto)
        {
            _logger.LogInformation("Creating new enrollment for Student {StudentId} in Course {CourseId}", dto.StudentId, dto.CourseId);
            var enrollment = new Enrollment
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                TeacherId = dto.TeacherId,
                Semester = dto.Semester,
                Year = dto.Year,
                Status = dto.Status
            };
            await _enrollmentRepository.AddAsync(enrollment);

            // Invalidate caches related to this student and course
            await InvalidateRelatedCaches(dto.StudentId, dto.CourseId);
            _logger.LogInformation("Successfully created enrollment. Related caches invalidated for Student {StudentId} and Course {CourseId}.", dto.StudentId, dto.CourseId);

            return true;
        }

        public async Task<bool> UpdateAsync(EnrollmentUpdateDto dto)
        {
            _logger.LogInformation("Updating enrollment ID: {EnrollmentId}", dto.EnrollmentId);
            var enrollment = await _enrollmentRepository.GetByIdAsync(dto.EnrollmentId);
            if (enrollment == null)
            {
                _logger.LogWarning("Update failed. Enrollment {EnrollmentId} not found.", dto.EnrollmentId);
                return false;
            }

            enrollment.StudentId = dto.StudentId;
            enrollment.CourseId = dto.CourseId;
            enrollment.TeacherId = dto.TeacherId;
            enrollment.Semester = dto.Semester;
            enrollment.Year = dto.Year;
            enrollment.Status = dto.Status;

            await _enrollmentRepository.UpdateAsync(enrollment);

            // Invalidate all related caches
            await _cacheService.RemoveDataAsync($"{CachePrefix}:{dto.EnrollmentId}");
            await InvalidateRelatedCaches(dto.StudentId, dto.CourseId);
            _logger.LogInformation("Successfully updated enrollment {EnrollmentId}. All related caches invalidated.", dto.EnrollmentId);

            return true;
        }

        public async Task<bool> DeleteAsync(int enrollmentId)
        {
            _logger.LogInformation("Deleting enrollment ID: {EnrollmentId}", enrollmentId);
            var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
            if (enrollment == null)
            {
                _logger.LogWarning("Delete failed. Enrollment {EnrollmentId} not found.", enrollmentId);
                return false;
            }

            await _enrollmentRepository.DeleteAsync(enrollment);

            // Invalidate all related caches
            await _cacheService.RemoveDataAsync($"{CachePrefix}:{enrollmentId}");
            await InvalidateRelatedCaches(enrollment.StudentId, enrollment.CourseId);
            _logger.LogInformation("Successfully deleted enrollment {EnrollmentId}. All related caches invalidated.", enrollmentId);

            return true;
        }

        private Task InvalidateRelatedCaches(string studentId, string courseId)
        {
            var studentCacheKey = $"{CachePrefix}:student:{studentId}";
            var courseCacheKey = $"{CachePrefix}:course:{courseId}";

            var tasks = new List<Task>
            {
                _cacheService.RemoveDataAsync(studentCacheKey),
                _cacheService.RemoveDataAsync(courseCacheKey)
            };

            return Task.WhenAll(tasks);
        }

        // Other methods
        public async Task<IEnumerable<EnrollmentDto>> SearchAsync(string searchTerm)
        {
            var enrollments = await _enrollmentRepository.SearchAsync(searchTerm);
            return enrollments.Select(MapToDto);
        }

        public async Task<(IEnumerable<EnrollmentDto> Enrollments, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (enrollments, totalCount) = await _enrollmentRepository.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return (enrollments.Select(MapToDto), totalCount);
        }
        public async Task<IEnumerable<EnrollmentDto>> GetUnscoredAsync()
        {
            var enrollments = await _enrollmentRepository.GetUnscoredAsync();
            return enrollments.Select(MapToDto); // Giả sử bạn đã có hàm MapToDto
        }
        public async Task<IEnumerable<EnrollmentWithScoreDto>> GetStudentEnrollmentsWithScoresAsync(string studentId)
        {
            _logger.LogInformation("Getting enrollments with scores for Student ID: {StudentId}", studentId);
            var cacheKey = $"{CachePrefix}:student:{studentId}:with-scores";

            var cachedData = await _cacheService.GetDataAsync<IEnumerable<EnrollmentWithScoreDto>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Enrollments with scores for Student ID {StudentId} found in cache.", studentId);
                return cachedData;
            }

            var enrollments = await _enrollmentRepository.GetStudentEnrollmentsWithScoresAsync(studentId);
            var dtos = enrollments.Select(MapToEnrollmentWithScoreDto).ToList();

            await _cacheService.SetDataAsync(cacheKey, dtos, System.DateTimeOffset.Now.AddMinutes(5));
            return dtos;
        }


        public async Task<IEnumerable<EnrollmentDto>> GetUnscoredEnrollmentsForClassAsync(string courseId, string teacherId)
        {
            _logger.LogInformation("Getting unscored enrollments for Course ID: {CourseId} and Teacher ID: {TeacherId}", courseId, teacherId);
            var cacheKey = $"{CachePrefix}:unscored:{courseId}:{teacherId}";

            var cachedData = await _cacheService.GetDataAsync<IEnumerable<EnrollmentDto>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Unscored enrollments for class {CourseId}/{TeacherId} found in cache.", courseId, teacherId);
                return cachedData;
            }

            var enrollments = await _enrollmentRepository.GetUnscoredEnrollmentsForClassAsync(courseId, teacherId);
            var dtos = enrollments.Select(MapToDto).ToList();

            await _cacheService.SetDataAsync(cacheKey, dtos, System.DateTimeOffset.Now.AddMinutes(1)); // Cache ngắn hơn vì dữ liệu này thay đổi thường xuyên
            _logger.LogInformation("Returning {Count} unscored enrollments for class {CourseId}/{TeacherId} from database.", dtos.Count, courseId, teacherId);

            return dtos;
        }

        // Helper method để map sang DTO mới - VERSION SIMPLE
        private static EnrollmentWithScoreDto MapToEnrollmentWithScoreDto(Enrollment e) => new EnrollmentWithScoreDto
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId ?? string.Empty,
            CourseId = e.CourseId ?? string.Empty,
            CourseName = e.Course?.CourseName ?? string.Empty,
            Credits = e.Course?.Credits ?? 0,
            TeacherId = e.TeacherId ?? string.Empty,

            // Tạm thời để trống TeacherName, sẽ cập nhật sau khi biết cấu trúc Teacher model
            TeacherName = string.Empty,

            Semester = e.Semester ?? string.Empty,
            Year = e.Year ?? 0,
            Status = e.Status,

            // Mapping điểm từ Score navigation property
            ProcessScore = e.Score?.ProcessScore,
            MidtermScore = e.Score?.MidtermScore,
            FinalScore = e.Score?.FinalScore,
            TotalScore = e.Score?.TotalScore,
            IsPassed = e.Score?.IsPassed ?? false,
            Grade = CalculateGrade(e.Score?.TotalScore)
        };

// Helper method tính grade
        private static string CalculateGrade(decimal? totalScore)
        {
            if (totalScore == null) return "N/A";

            return totalScore switch
            {
                >= 9.0m => "A+",
                >= 8.5m => "A",
                >= 8.0m => "B+",
                >= 7.0m => "B",
                >= 6.5m => "C+",
                >= 5.5m => "C",
                >= 4.0m => "D",
                _ => "F"
            };
        }
        private static EnrollmentDto MapToDto(Enrollment e) => new EnrollmentDto
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            CourseId = e.CourseId,
            TeacherId = e.TeacherId,
            Semester = e.Semester,
            Year = e.Year,
            Status = e.Status,

        };
    }
}
