using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Dtos.Course;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<CourseDto?> GetByIdAsync(string courseId)
        {
            var course = await _courseRepository.GetCourseWithEnrollmentsAsync(courseId);
            if (course == null) return null;
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

        public async Task<IEnumerable<CourseListDto>> GetAllAsync()
        {
            var courses = await _courseRepository.GetAllAsync();
            return courses.Select(c => new CourseListDto
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                Credits = c.Credits,
                Department = c.Department,
                IsActive = c.IsActive
            });
        }

        public async Task<IEnumerable<CourseListDto>> GetActiveCoursesAsync()
        {
            var courses = await _courseRepository.GetActiveCoursesAsync();
            return courses.Select(c => new CourseListDto
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                Credits = c.Credits,
                Department = c.Department,
                IsActive = c.IsActive
            });
        }

        public async Task<bool> IsCourseIdExistsAsync(string courseId)
        {
            return await _courseRepository.IsCourseIdExistsAsync(courseId);
        }

        public async Task<bool> IsCourseNameExistsAsync(string courseName, string? excludeCourseId = null)
        {
            return await _courseRepository.IsCourseNameExistsAsync(courseName, excludeCourseId);
        }

        public async Task<IEnumerable<CourseListDto>> SearchCoursesAsync(string searchTerm)
        {
            var courses = await _courseRepository.SearchCoursesAsync(searchTerm);
            return courses.Select(c => new CourseListDto
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                Credits = c.Credits,
                Department = c.Department,
                IsActive = c.IsActive
            });
        }

        public async Task<(IEnumerable<CourseListDto> Courses, int TotalCount)> GetPagedCoursesAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            var (courses, totalCount) = await _courseRepository.GetPagedCoursesAsync(pageNumber, pageSize, searchTerm, isActive);
            var dtos = courses.Select(c => new CourseListDto
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                Credits = c.Credits,
                Department = c.Department,
                IsActive = c.IsActive
            });
            return (dtos, totalCount);
        }

        public async Task<IEnumerable<CourseListDto>> GetCoursesByDepartmentAsync(string department)
        {
            var courses = await _courseRepository.GetCoursesByDepartmentAsync(department);
            return courses.Select(c => new CourseListDto
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                Credits = c.Credits,
                Department = c.Department,
                IsActive = c.IsActive
            });
        }

        public async Task<int> GetEnrollmentCountInCourseAsync(string courseId)
        {
            return await _courseRepository.GetEnrollmentCountInCourseAsync(courseId);
        }

        public async Task<bool> CreateAsync(CourseCreateDto dto)
        {
            if (await _courseRepository.IsCourseIdExistsAsync(dto.CourseId)) return false;
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
            return true;
        }

        public async Task<bool> UpdateAsync(CourseUpdateDto dto)
        {
            var course = await _courseRepository.GetByIdAsync(dto.CourseId);
            if (course == null) return false;
            course.CourseName = dto.CourseName;
            course.Credits = dto.Credits;
            course.Department = dto.Department;
            course.Description = dto.Description;
            course.IsActive = dto.IsActive;
            await _courseRepository.UpdateAsync(course);
            return true;
        }

        public async Task<bool> DeleteAsync(string courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return false;
            await _courseRepository.DeleteAsync(course);
            return true;
        }
    }
}