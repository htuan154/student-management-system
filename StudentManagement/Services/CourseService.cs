using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Course;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CourseService> _logger;
        private const string CourseCachePrefix = "course";

        public CourseService(ICourseRepository courseRepository, ICacheService cacheService, ILogger<CourseService> logger)
        {
            _courseRepository = courseRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<CourseDto?> GetByIdAsync(string courseId)
        {
            _logger.LogInformation("Getting course by ID: {CourseId}", courseId);
            var cacheKey = $"{CourseCachePrefix}:{courseId}";

            var cachedCourse = await _cacheService.GetDataAsync<CourseDto>(cacheKey);
            if (cachedCourse != null)
            {
                _logger.LogInformation("Course {CourseId} found in cache.", courseId);
                return cachedCourse;
            }

            _logger.LogInformation("Course {CourseId} not in cache. Fetching from database.", courseId);
            var course = await _courseRepository.GetCourseWithEnrollmentsAsync(courseId);
            if (course == null) return null;

            var courseDto = MapToDto(course);
            await _cacheService.SetDataAsync(cacheKey, courseDto, System.DateTimeOffset.Now.AddMinutes(5));

            return courseDto;
        }

        public async Task<IEnumerable<CourseListDto>> GetAllAsync()
        {
            _logger.LogInformation("Getting all courses.");
            // Caching for full lists can be complex.
            // A simple implementation would cache the entire list under a specific key.
            var courses = await _courseRepository.GetAllAsync();
            return courses.Select(MapToListDto);
        }

        public async Task<bool> CreateAsync(CourseCreateDto dto)
        {
            _logger.LogInformation("Creating new course with ID: {CourseId}", dto.CourseId);
            if (await _courseRepository.IsCourseIdExistsAsync(dto.CourseId))
            {
                _logger.LogWarning("Create failed. Course ID {CourseId} already exists.", dto.CourseId);
                return false;
            }

            var course = new Course
            {
                CourseId = dto.CourseId,
                CourseName = dto.CourseName,
                Credits = dto.Credits,
                Department = dto.Department,
                Description = dto.Description,
                IsActive = dto.IsActive
            };
            await _courseRepository.AddAsync(course);
            _logger.LogInformation("Course {CourseId} created successfully.", dto.CourseId);

            // Invalidate any list-based caches
            // Example: await _cacheService.RemoveDataAsync("courses-paged:*");

            return true;
        }

        public async Task<bool> UpdateAsync(CourseUpdateDto dto)
        {
            _logger.LogInformation("Updating course with ID: {CourseId}", dto.CourseId);
            var course = await _courseRepository.GetByIdAsync(dto.CourseId);
            if (course == null)
            {
                _logger.LogWarning("Update failed. Course with ID {CourseId} not found.", dto.CourseId);
                return false;
            }

            course.CourseName = dto.CourseName;
            course.Credits = dto.Credits;
            course.Department = dto.Department;
            course.Description = dto.Description;
            course.IsActive = dto.IsActive;

            await _courseRepository.UpdateAsync(course);
            _logger.LogInformation("Course {CourseId} updated successfully.", dto.CourseId);

            // Invalidate the cache for this specific course
            var cacheKey = $"{CourseCachePrefix}:{dto.CourseId}";
            await _cacheService.RemoveDataAsync(cacheKey);
            _logger.LogInformation("Cache for course {CourseId} invalidated.", dto.CourseId);

            return true;
        }

        public async Task<bool> DeleteAsync(string courseId)
        {
            _logger.LogInformation("Deleting course with ID: {CourseId}", courseId);
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("Delete failed. Course with ID {CourseId} not found.", courseId);
                return false;
            }

            // Add business logic check if needed, e.g., cannot delete if students are enrolled.

            await _courseRepository.DeleteAsync(course);
            _logger.LogInformation("Course {CourseId} deleted successfully.", courseId);

            // Invalidate the cache
            var cacheKey = $"{CourseCachePrefix}:{courseId}";
            await _cacheService.RemoveDataAsync(cacheKey);
            _logger.LogInformation("Cache for course {CourseId} invalidated.", courseId);

            return true;
        }

        // Other public methods with logging added
        public async Task<IEnumerable<CourseListDto>> GetActiveCoursesAsync()
        {
            _logger.LogInformation("Getting active courses.");
            var courses = await _courseRepository.GetActiveCoursesAsync();
            return courses.Select(MapToListDto);
        }

        public async Task<bool> IsCourseIdExistsAsync(string courseId) => await _courseRepository.IsCourseIdExistsAsync(courseId);

        public async Task<bool> IsCourseNameExistsAsync(string courseName, string? excludeCourseId = null) => await _courseRepository.IsCourseNameExistsAsync(courseName, excludeCourseId);

        public async Task<IEnumerable<CourseListDto>> SearchCoursesAsync(string searchTerm)
        {
            _logger.LogInformation("Searching courses with term: {SearchTerm}", searchTerm);
            var courses = await _courseRepository.SearchCoursesAsync(searchTerm);
            return courses.Select(MapToListDto);
        }

        public async Task<(IEnumerable<CourseListDto> Courses, int TotalCount)> GetPagedCoursesAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            _logger.LogInformation("Getting paged courses. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            var (courses, totalCount) = await _courseRepository.GetPagedCoursesAsync(pageNumber, pageSize, searchTerm, isActive);
            var dtos = courses.Select(MapToListDto);
            return (dtos, totalCount);
        }

        public async Task<IEnumerable<CourseListDto>> GetCoursesByDepartmentAsync(string department)
        {
            _logger.LogInformation("Getting courses by department: {Department}", department);
            var courses = await _courseRepository.GetCoursesByDepartmentAsync(department);
            return courses.Select(MapToListDto);
        }

        public async Task<int> GetEnrollmentCountInCourseAsync(string courseId) => await _courseRepository.GetEnrollmentCountInCourseAsync(courseId);

        #region Mapping Methods
        private static CourseDto MapToDto(Course course)
        {
            return new CourseDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Credits = course.Credits,
                Department = course.Department,
                Description = course.Description,
                IsActive = course.IsActive
            };
        }

        private static CourseListDto MapToListDto(Course course)
        {
            return new CourseListDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Credits = course.Credits,
                Department = course.Department,
                IsActive = course.IsActive
            };
        }
        #endregion
    }
}
