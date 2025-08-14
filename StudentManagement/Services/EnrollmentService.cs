using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Enrollment;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

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
            await _cacheService.SetDataAsync(cacheKey, dto, DateTimeOffset.Now.AddMinutes(5));
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
            await _cacheService.SetDataAsync(cacheKey, dtos, DateTimeOffset.Now.AddMinutes(5));
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
            await _cacheService.SetDataAsync(cacheKey, dtos, DateTimeOffset.Now.AddMinutes(5));
            return dtos;
        }

        // ✅ THÊM - Method mới cho TeacherCourse
        public async Task<IEnumerable<EnrollmentDto>> GetByTeacherCourseIdAsync(int teacherCourseId)
        {
            _logger.LogInformation("Getting enrollments for TeacherCourse ID: {TeacherCourseId}", teacherCourseId);
            var cacheKey = $"{CachePrefix}:teachercourse:{teacherCourseId}";

            var cachedData = await _cacheService.GetDataAsync<IEnumerable<EnrollmentDto>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Enrollments for TeacherCourse ID {TeacherCourseId} found in cache.", teacherCourseId);
                return cachedData;
            }

            var enrollments = await _enrollmentRepository.GetByTeacherCourseIdAsync(teacherCourseId);
            var dtos = enrollments.Select(MapToDto).ToList();
            await _cacheService.SetDataAsync(cacheKey, dtos, DateTimeOffset.Now.AddMinutes(5));
            return dtos;
        }

        // ✅ THÊM - Method cho Semester
        public async Task<IEnumerable<EnrollmentDto>> GetBySemesterIdAsync(int semesterId)
        {
            _logger.LogInformation("Getting enrollments for Semester ID: {SemesterId}", semesterId);
            var cacheKey = $"{CachePrefix}:semester:{semesterId}";

            var cachedData = await _cacheService.GetDataAsync<IEnumerable<EnrollmentDto>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Enrollments for Semester ID {SemesterId} found in cache.", semesterId);
                return cachedData;
            }

            var enrollments = await _enrollmentRepository.GetBySemesterIdAsync(semesterId);
            var dtos = enrollments.Select(MapToDto).ToList();
            await _cacheService.SetDataAsync(cacheKey, dtos, DateTimeOffset.Now.AddMinutes(5));
            return dtos;
        }

        // ✅ SỬA - CreateAsync với schema mới
        public async Task<bool> CreateAsync(EnrollmentCreateDto dto)
        {
            _logger.LogInformation("Creating new enrollment for Student {StudentId} in TeacherCourse {TeacherCourseId}",
                dto.StudentId, dto.TeacherCourseId);

            var enrollment = new Enrollment
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                TeacherCourseId = dto.TeacherCourseId, // ✅ SỬA - Sử dụng TeacherCourseId
                SemesterId = dto.SemesterId, // ✅ SỬA - Sử dụng SemesterId
                Status = dto.Status
            };

            await _enrollmentRepository.AddAsync(enrollment);

            // Invalidate caches
            await InvalidateRelatedCaches(dto.StudentId, dto.CourseId, dto.TeacherCourseId, dto.SemesterId);
            _logger.LogInformation("Successfully created enrollment. Related caches invalidated.");

            return true;
        }

        // ✅ SỬA - UpdateAsync với schema mới
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
            enrollment.TeacherCourseId = dto.TeacherCourseId; // ✅ SỬA
            enrollment.SemesterId = dto.SemesterId; // ✅ SỬA
            enrollment.Status = dto.Status;

            await _enrollmentRepository.UpdateAsync(enrollment);

            // Invalidate all related caches
            await _cacheService.RemoveDataAsync($"{CachePrefix}:{dto.EnrollmentId}");
            await InvalidateRelatedCaches(dto.StudentId, dto.CourseId, dto.TeacherCourseId, dto.SemesterId);
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
            await InvalidateRelatedCaches(enrollment.StudentId, enrollment.CourseId,
                enrollment.TeacherCourseId, enrollment.SemesterId);
            _logger.LogInformation("Successfully deleted enrollment {EnrollmentId}. All related caches invalidated.", enrollmentId);

            return true;
        }

        // ✅ SỬA - InvalidateRelatedCaches với parameters mới
        private Task InvalidateRelatedCaches(string studentId, string courseId, int? teacherCourseId, int? semesterId)
        {
            var tasks = new List<Task>
            {
                _cacheService.RemoveDataAsync($"{CachePrefix}:student:{studentId}"),
                _cacheService.RemoveDataAsync($"{CachePrefix}:course:{courseId}")
            };

            if (teacherCourseId.HasValue)
            {
                tasks.Add(_cacheService.RemoveDataAsync($"{CachePrefix}:teachercourse:{teacherCourseId}"));
            }

            if (semesterId.HasValue)
            {
                tasks.Add(_cacheService.RemoveDataAsync($"{CachePrefix}:semester:{semesterId}"));
            }

            return Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<EnrollmentDto>> SearchAsync(string searchTerm)
        {
            var enrollments = await _enrollmentRepository.SearchAsync(searchTerm);
            return enrollments.Select(MapToDto);
        }

        public async Task<(IEnumerable<EnrollmentDto> Enrollments, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (enrollments, totalCount) = await _enrollmentRepository.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return (enrollments.Select(MapToDto), totalCount);
        }

        public async Task<IEnumerable<EnrollmentDto>> GetUnscoredAsync()
        {
            var enrollments = await _enrollmentRepository.GetUnscoredAsync();
            return enrollments.Select(MapToDto);
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

            await _cacheService.SetDataAsync(cacheKey, dtos, DateTimeOffset.Now.AddMinutes(5));
            return dtos;
        }

        // ✅ SỬA - GetUnscoredEnrollmentsForClassAsync với TeacherCourseId
        public async Task<IEnumerable<EnrollmentDto>> GetUnscoredEnrollmentsForClassAsync(int teacherCourseId)
        {
            _logger.LogInformation("Getting unscored enrollments for TeacherCourse ID: {TeacherCourseId}", teacherCourseId);
            var cacheKey = $"{CachePrefix}:unscored:teachercourse:{teacherCourseId}";

            var cachedData = await _cacheService.GetDataAsync<IEnumerable<EnrollmentDto>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Unscored enrollments for TeacherCourse {TeacherCourseId} found in cache.", teacherCourseId);
                return cachedData;
            }

            // ✅ SỬA - Gọi đúng method name
            var enrollments = await _enrollmentRepository.GetUnscoredEnrollmentsByTeacherCourseAsync(teacherCourseId);
            var dtos = enrollments.Select(MapToDto).ToList();

            await _cacheService.SetDataAsync(cacheKey, dtos, DateTimeOffset.Now.AddMinutes(1));
            _logger.LogInformation("Returning {Count} unscored enrollments for TeacherCourse {TeacherCourseId} from database.", 
                dtos.Count, teacherCourseId);

            return dtos;
        }


        private static EnrollmentWithScoreDto MapToEnrollmentWithScoreDto(Enrollment e) => new EnrollmentWithScoreDto
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId ?? string.Empty,
            StudentName = e.Student?.FullName ?? string.Empty,
            CourseId = e.CourseId ?? string.Empty,
            CourseName = e.Course?.CourseName ?? string.Empty,
            Credits = e.Course?.Credits ?? 0,


            TeacherId = e.TeacherCourse?.TeacherId ?? string.Empty,
            TeacherName = e.TeacherCourse?.Teacher?.FullName ?? string.Empty,

            SemesterId = e.SemesterId,
            SemesterName = e.Semester?.SemesterName ?? string.Empty,
            AcademicYear = e.Semester?.AcademicYear ?? string.Empty,

            Status = e.Status,

            // Mapping điểm từ Score navigation property
            ProcessScore = e.Score?.ProcessScore,
            MidtermScore = e.Score?.MidtermScore,
            FinalScore = e.Score?.FinalScore,
            TotalScore = e.Score?.TotalScore,
            IsPassed = e.Score?.TotalScore >= 4,
            Grade = CalculateGrade(e.Score?.TotalScore)
        };

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
            StudentName = e.Student?.FullName,
            CourseId = e.CourseId,
            CourseName = e.Course?.CourseName,
            TeacherCourseId = e.TeacherCourseId,
            TeacherId = e.TeacherCourse?.TeacherId,
            TeacherName = e.TeacherCourse?.Teacher?.FullName,
            SemesterId = e.SemesterId,
            SemesterName = e.Semester?.SemesterName,
            AcademicYear = e.Semester?.AcademicYear, 
            Status = e.Status
        };
    }
}
