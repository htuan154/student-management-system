using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student?> GetStudentWithClassAsync(string studentId);
        Task<IEnumerable<Student>> GetStudentsByClassIdAsync(string classId);
        Task<bool> IsEmailExistsAsync(string email, string? excludeStudentId = null);
        Task<bool> IsStudentIdExistsAsync(string studentId);
        Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm);
        Task<(IEnumerable<Student> Students, int TotalCount)> GetPagedStudentsAsync(int pageNumber, int pageSize, string? searchTerm = null);
    }
}
