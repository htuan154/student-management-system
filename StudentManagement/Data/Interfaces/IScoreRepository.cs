using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IScoreRepository : IRepository<Score>
    {
        Task<Score?> GetByEnrollmentIdAsync(int enrollmentId);
        Task<IEnumerable<Score>> SearchScoresAsync(string searchTerm);
        Task<(IEnumerable<Score> Scores, int TotalCount)> GetPagedScoresAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<Score>> GetByTeacherAndSubjectAsync(string teacherId, string courseId);


    }
}
