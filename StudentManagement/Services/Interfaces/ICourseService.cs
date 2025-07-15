using StudentManagementSystem.Dtos.Course;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface ICourseService
    {
        Task<CourseDto?> GetByIdAsync(string courseId);
        Task<IEnumerable<CourseListDto>> GetAllAsync();
        Task<IEnumerable<CourseListDto>> GetActiveCoursesAsync();
        Task<bool> IsCourseIdExistsAsync(string courseId);
        Task<bool> IsCourseNameExistsAsync(string courseName, string? excludeCourseId = null);
        Task<IEnumerable<CourseListDto>> SearchCoursesAsync(string searchTerm);
        Task<(IEnumerable<CourseListDto> Courses, int TotalCount)> GetPagedCoursesAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<IEnumerable<CourseListDto>> GetCoursesByDepartmentAsync(string department);
        Task<int> GetEnrollmentCountInCourseAsync(string courseId);
        Task<bool> CreateAsync(CourseCreateDto dto);
        Task<bool> UpdateAsync(CourseUpdateDto dto);
        Task<bool> DeleteAsync(string courseId);
    }
}
