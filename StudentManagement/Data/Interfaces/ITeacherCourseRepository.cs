using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface ITeacherCourseRepository : IRepository<TeacherCourse>
    {
        Task<IEnumerable<TeacherCourse>> GetByTeacherIdAsync(string teacherId);
        Task<IEnumerable<TeacherCourse>> GetByCourseIdAsync(string courseId);
        Task<IEnumerable<TeacherCourse>> SearchAsync(string searchTerm);
        Task<(IEnumerable<TeacherCourse> TeacherCourses, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
    }
}