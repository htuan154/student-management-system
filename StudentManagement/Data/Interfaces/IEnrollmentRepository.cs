using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<IEnumerable<Enrollment>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<Enrollment>> GetByCourseIdAsync(string courseId);
        Task<IEnumerable<Enrollment>> SearchAsync(string searchTerm);
        Task<(IEnumerable<Enrollment> Enrollments, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<Enrollment>> GetUnscoredAsync();
    }
}
