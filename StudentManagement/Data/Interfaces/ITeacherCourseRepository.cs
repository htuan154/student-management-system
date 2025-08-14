using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface ITeacherCourseRepository : IRepository<TeacherCourse>
    {
        Task<TeacherCourse?> GetWithDetailsAsync(int id);
        Task<IEnumerable<TeacherCourse>> GetAllWithDetailsAsync();
        Task<IEnumerable<TeacherCourse>> GetByTeacherIdAsync(string teacherId);
        Task<IEnumerable<TeacherCourse>> GetByCourseIdAsync(string courseId);
        Task<IEnumerable<TeacherCourse>> GetBySemesterIdAsync(int semesterId);
        Task<IEnumerable<TeacherCourse>> GetActiveTeacherCoursesAsync();
        Task<IEnumerable<TeacherCourse>> SearchAsync(string searchTerm);
        Task<(IEnumerable<TeacherCourse> TeacherCourses, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<bool> IsTeacherCourseExistsAsync(string teacherId, string courseId, int semesterId, int? excludeId = null);
        Task<IEnumerable<TeacherCourse>> GetTeacherCoursesBySemesterAsync(int semesterId, bool? isActive = null);
        Task<int> GetTeacherWorkloadAsync(string teacherId, int semesterId);
        Task<int> GetCourseInstructorsCountAsync(string courseId, int semesterId);
    }
}
