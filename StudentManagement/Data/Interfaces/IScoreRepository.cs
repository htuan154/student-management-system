using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IScoreRepository : IRepository<Score>
    {
        Task<Score?> GetByEnrollmentIdAsync(int enrollmentId);
        Task<IEnumerable<Score>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<Score>> GetByTeacherAndCourseAsync(string teacherId, string courseId);
        Task<IEnumerable<Score>> GetByTeacherAndSubjectAsync(string teacherId, string courseId);
        Task<IEnumerable<Score>> GetByTeacherCourseIdAsync(int teacherCourseId);
        Task<IEnumerable<Score>> GetByTeacherIdAsync(string teacherId);
        Task<IEnumerable<Score>> GetByCourseIdAsync(string courseId);
        Task<IEnumerable<Score>> GetBySemesterIdAsync(int semesterId);
        Task<IEnumerable<Score>> SearchScoresAsync(string searchTerm);
        Task<(IEnumerable<Score> Scores, int TotalCount)> GetPagedScoresAsync(
            int pageNumber, int pageSize, string? searchTerm = null, string? courseId = null, int? semesterId = null);
        Task<decimal> GetAverageScoreByStudentAsync(string studentId);
        Task<decimal> GetAverageScoreByCourseAsync(string courseId);
        Task<decimal> GetAverageScoreBySemesterAsync(int semesterId);
        Task<decimal> GetAverageScoreByTeacherAsync(string teacherId);
        Task<IEnumerable<Score>> GetTopScoresByCourseAsync(string courseId, int topCount = 10);
        Task<int> GetPassingStudentCountByCourseAsync(string courseId, decimal passingScore = 5.0m);
        Task<int> GetFailingStudentCountByCourseAsync(string courseId, decimal passingScore = 5.0m);
        Task<Dictionary<string, int>> GetGradeDistributionByCourseAsync(string courseId);
        Task<bool> HasScoreAsync(int enrollmentId);
        Task<bool> IsScoreCompleteAsync(int enrollmentId);
    }
}
