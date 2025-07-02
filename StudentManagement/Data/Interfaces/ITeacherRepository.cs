using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface ITeacherRepository : IRepository<Teacher>
    {
        Task<Teacher?> GetTeacherWithUsersAsync(string teacherId);
        Task<Teacher?> GetTeacherWithCoursesAsync(string teacherId);
        Task<Teacher?> GetTeacherWithEnrollmentsAsync(string teacherId);
        Task<Teacher?> GetTeacherFullDetailsAsync(string teacherId);
        Task<IEnumerable<Teacher>> GetTeachersByDepartmentAsync(string department);
        Task<IEnumerable<Teacher>> GetTeachersByDegreeAsync(string degree);
        Task<bool> IsEmailExistsAsync(string email, string? excludeTeacherId = null);
        Task<bool> IsTeacherIdExistsAsync(string teacherId);
        Task<IEnumerable<Teacher>> SearchTeachersAsync(string searchTerm);
        Task<(IEnumerable<Teacher> Teachers, int TotalCount)> GetPagedTeachersAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<Teacher>> GetTeachersBySalaryRangeAsync(decimal? minSalary, decimal? maxSalary);
        Task<IEnumerable<string>> GetDistinctDepartmentsAsync();
        Task<IEnumerable<string>> GetDistinctDegreesAsync();
        Task<IEnumerable<Teacher>> GetTeachersWithActiveCourses();
        Task<int> GetTeacherCourseCountAsync(string teacherId);
        Task<int> GetTeacherEnrollmentCountAsync(string teacherId);
    }
}
