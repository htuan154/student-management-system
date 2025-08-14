using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.TeacherCourse;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public class TeacherCourseService : ITeacherCourseService
    {
        private readonly ITeacherCourseRepository _teacherCourseRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<TeacherCourseService> _logger;
        private const string CachePrefix = "teachercourse";

        public TeacherCourseService(ITeacherCourseRepository teacherCourseRepository, ICacheService cacheService, ILogger<TeacherCourseService> logger)
        {
            _teacherCourseRepository = teacherCourseRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<TeacherCourseDto?> GetByIdAsync(int teacherCourseId)
        {
            _logger.LogInformation("Getting TeacherCourse by ID: {Id}", teacherCourseId);
            var cacheKey = $"{CachePrefix}:{teacherCourseId}";
            var cachedData = await _cacheService.GetDataAsync<TeacherCourseDto>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Data found in cache for key: {CacheKey}", cacheKey);
                return cachedData;
            }

            var tc = await _teacherCourseRepository.GetByIdAsync(teacherCourseId);
            if (tc == null) return null;

            var dto = MapToDto(tc);
            await _cacheService.SetDataAsync(cacheKey, dto, System.DateTimeOffset.Now.AddMinutes(15));
            return dto;
        }

        public async Task<IEnumerable<TeacherCourseDto>> GetByTeacherIdAsync(string teacherId)
        {
            _logger.LogInformation("Getting courses for Teacher ID: {TeacherId}", teacherId);
            var cacheKey = $"{CachePrefix}:teacher:{teacherId}";
            var cachedData = await _cacheService.GetDataAsync<IEnumerable<TeacherCourseDto>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Data found in cache for key: {CacheKey}", cacheKey);
                return cachedData;
            }

            var tcs = await _teacherCourseRepository.GetByTeacherIdAsync(teacherId);
            var dtos = tcs.Select(MapToDto).ToList();
            await _cacheService.SetDataAsync(cacheKey, dtos, System.DateTimeOffset.Now.AddMinutes(15));
            return dtos;
        }

        public async Task<IEnumerable<TeacherCourseDto>> GetByCourseIdAsync(string courseId)
        {
            _logger.LogInformation("Getting teachers for Course ID: {CourseId}", courseId);
            var cacheKey = $"{CachePrefix}:course:{courseId}";
            var cachedData = await _cacheService.GetDataAsync<IEnumerable<TeacherCourseDto>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Data found in cache for key: {CacheKey}", cacheKey);
                return cachedData;
            }

            var tcs = await _teacherCourseRepository.GetByCourseIdAsync(courseId);
            var dtos = tcs.Select(MapToDto).ToList();
            await _cacheService.SetDataAsync(cacheKey, dtos, System.DateTimeOffset.Now.AddMinutes(15));
            return dtos;
        }

        public async Task<IEnumerable<TeacherCourseDto>> GetBySemesterIdAsync(int semesterId)
        {
            _logger.LogInformation("Getting teacher courses for Semester ID: {SemesterId}", semesterId);
            var cacheKey = $"{CachePrefix}:semester:{semesterId}";
            var cachedData = await _cacheService.GetDataAsync<IEnumerable<TeacherCourseDto>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Data found in cache for key: {CacheKey}", cacheKey);
                return cachedData;
            }

            var tcs = await _teacherCourseRepository.GetBySemesterIdAsync(semesterId);
            var dtos = tcs.Select(MapToDto).ToList();
            await _cacheService.SetDataAsync(cacheKey, dtos, System.DateTimeOffset.Now.AddMinutes(15));
            return dtos;
        }

        public async Task<bool> CreateAsync(TeacherCourseCreateDto dto)
        {
            _logger.LogInformation("Assigning Teacher {TeacherId} to Course {CourseId} in Semester {SemesterId}",
                dto.TeacherId, dto.CourseId, dto.SemesterId);

            var tc = new TeacherCourse
            {
                TeacherId = dto.TeacherId,
                CourseId = dto.CourseId,
                SemesterId = dto.SemesterId,
                IsActive = dto.IsActive
            };
            await _teacherCourseRepository.AddAsync(tc);

            await InvalidateTeacherCourseCaches(dto.TeacherId, dto.CourseId, dto.SemesterId);
            _logger.LogInformation("Assignment created. Caches invalidated for Teacher {TeacherId} and Course {CourseId}.",
                dto.TeacherId, dto.CourseId);
            return true;
        }

        public async Task<bool> UpdateAsync(TeacherCourseUpdateDto dto)
        {
            _logger.LogInformation("Updating TeacherCourse assignment ID: {Id}", dto.TeacherCourseId);
            var tc = await _teacherCourseRepository.GetByIdAsync(dto.TeacherCourseId);
            if (tc == null)
            {
                _logger.LogWarning("Update failed. Assignment {Id} not found.", dto.TeacherCourseId);
                return false;
            }

            await InvalidateTeacherCourseCaches(tc.TeacherId, tc.CourseId, tc.SemesterId);

            tc.TeacherId = dto.TeacherId;
            tc.CourseId = dto.CourseId;
            tc.SemesterId = dto.SemesterId;
            tc.IsActive = dto.IsActive;
            await _teacherCourseRepository.UpdateAsync(tc);

            await _cacheService.RemoveDataAsync($"{CachePrefix}:{dto.TeacherCourseId}");
            await InvalidateTeacherCourseCaches(dto.TeacherId, dto.CourseId, dto.SemesterId);
            _logger.LogInformation("Assignment {Id} updated and related caches invalidated.", dto.TeacherCourseId);

            return true;
        }

        public async Task<bool> DeleteAsync(int teacherCourseId)
        {
            _logger.LogInformation("Deleting TeacherCourse assignment ID: {Id}", teacherCourseId);
            var tc = await _teacherCourseRepository.GetByIdAsync(teacherCourseId);
            if (tc == null)
            {
                _logger.LogWarning("Delete failed. Assignment {Id} not found.", teacherCourseId);
                return false;
            }

            await _teacherCourseRepository.DeleteAsync(tc);

            await _cacheService.RemoveDataAsync($"{CachePrefix}:{teacherCourseId}");
            await InvalidateTeacherCourseCaches(tc.TeacherId, tc.CourseId, tc.SemesterId);
            _logger.LogInformation("Assignment {Id} deleted and related caches invalidated.", teacherCourseId);

            return true;
        }

        private Task InvalidateTeacherCourseCaches(string teacherId, string courseId, int semesterId)
        {
            var teacherCacheKey = $"{CachePrefix}:teacher:{teacherId}";
            var courseCacheKey = $"{CachePrefix}:course:{courseId}";
            var semesterCacheKey = $"{CachePrefix}:semester:{semesterId}";

            var tasks = new List<Task>
            {
                _cacheService.RemoveDataAsync(teacherCacheKey),
                _cacheService.RemoveDataAsync(courseCacheKey),
                _cacheService.RemoveDataAsync(semesterCacheKey)
            };
            return Task.WhenAll(tasks);
        }

        // Other methods
        public async Task<IEnumerable<TeacherCourseDto>> SearchAsync(string searchTerm)
        {
            var tcs = await _teacherCourseRepository.SearchAsync(searchTerm);
            return tcs.Select(MapToDto);
        }

        public async Task<(IEnumerable<TeacherCourseDto> TeacherCourses, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (tcs, totalCount) = await _teacherCourseRepository.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return (tcs.Select(MapToDto), totalCount);
        }

        public async Task<bool> IsTeacherCourseExistsAsync(string teacherId, string courseId, int semesterId, int? excludeId = null)
        {
            return await _teacherCourseRepository.IsTeacherCourseExistsAsync(teacherId, courseId, semesterId, excludeId);
        }

        public async Task<int> GetTeacherWorkloadAsync(string teacherId, int semesterId)
        {
            return await _teacherCourseRepository.GetTeacherWorkloadAsync(teacherId, semesterId);
        }

        private static TeacherCourseDto MapToDto(TeacherCourse tc) => new TeacherCourseDto
        {
            TeacherCourseId = tc.TeacherCourseId,
            TeacherId = tc.TeacherId,
            TeacherName = tc.Teacher?.FullName,
            CourseId = tc.CourseId,
            CourseName = tc.Course?.CourseName,
            SemesterId = tc.SemesterId,
            SemesterName = tc.Semester?.SemesterName,
            AcademicYear = tc.Semester?.AcademicYear,
            IsActive = tc.IsActive
        };
    }
}
