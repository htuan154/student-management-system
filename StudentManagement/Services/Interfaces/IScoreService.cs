using StudentManagementSystem.DTOs.Score;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface IScoreService
    {
        Task<ScoreDto?> GetByIdAsync(int scoreId);
        Task<ScoreDto?> GetByEnrollmentIdAsync(int enrollmentId);
        Task<IEnumerable<ScoreDto>> SearchScoresAsync(string searchTerm);
        Task<(IEnumerable<ScoreDto> Scores, int TotalCount)> GetPagedScoresAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<bool> CreateAsync(ScoreCreateDto dto);
        Task<bool> UpdateAsync(ScoreUpdateDto dto);
        Task<bool> DeleteAsync(int scoreId);
        Task<IEnumerable<ScoreDto>> GetAllAsync();
        Task<IEnumerable<ScoreDetailsDto>> GetByTeacherAndSubjectAsync(string teacherId, string subjectId);
    }
}
