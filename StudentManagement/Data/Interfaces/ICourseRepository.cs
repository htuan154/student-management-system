using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<Course?> GetCourseWithEnrollmentsAsync(string courseId);
        Task<IEnumerable<Course>> GetActiveCoursesAsync();
        Task<bool> IsCourseIdExistsAsync(string courseId);
        Task<bool> IsCourseNameExistsAsync(string courseName, string? excludeCourseId = null);
        Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm);
        Task<(IEnumerable<Course> Courses, int TotalCount)> GetPagedCoursesAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(string department);
        Task<int> GetEnrollmentCountInCourseAsync(string courseId);
    }
}
