using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Dtos.Score;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class ScoreService : IScoreService
    {
        private readonly IScoreRepository _scoreRepository;

        public ScoreService(IScoreRepository scoreRepository)
        {
            _scoreRepository = scoreRepository;
        }

        public async Task<ScoreDto?> GetByIdAsync(int scoreId)
        {
            var score = await _scoreRepository.GetByIdAsync(scoreId);
            return score == null ? null : MapToDto(score);
        }

        public async Task<ScoreDto?> GetByEnrollmentIdAsync(int enrollmentId)
        {
            var score = await _scoreRepository.GetByEnrollmentIdAsync(enrollmentId);
            return score == null ? null : MapToDto(score);
        }

        public async Task<IEnumerable<ScoreDto>> SearchScoresAsync(string searchTerm)
        {
            var scores = await _scoreRepository.SearchScoresAsync(searchTerm);
            return scores.Select(MapToDto);
        }

        public async Task<(IEnumerable<ScoreDto> Scores, int TotalCount)> GetPagedScoresAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (scores, totalCount) = await _scoreRepository.GetPagedScoresAsync(pageNumber, pageSize, searchTerm);
            return (scores.Select(MapToDto), totalCount);
        }

        public async Task<bool> CreateAsync(ScoreCreateDto dto)
        {
            var score = new Score
            {
                EnrollmentId = dto.EnrollmentId,
                ProcessScore = dto.ProcessScore,
                MidtermScore = dto.MidtermScore,
                FinalScore = dto.FinalScore
            };
            await _scoreRepository.AddAsync(score);
            return true;
        }

        public async Task<bool> UpdateAsync(ScoreUpdateDto dto)
        {
            var score = await _scoreRepository.GetByIdAsync(dto.ScoreId);
            if (score == null) return false;
            score.EnrollmentId = dto.EnrollmentId;
            score.ProcessScore = dto.ProcessScore;
            score.MidtermScore = dto.MidtermScore;
            score.FinalScore = dto.FinalScore;
            await _scoreRepository.UpdateAsync(score);
            return true;
        }

        public async Task<bool> DeleteAsync(int scoreId)
        {
            var score = await _scoreRepository.GetByIdAsync(scoreId);
            if (score == null) return false;
            await _scoreRepository.DeleteAsync(score);
            return true;
        }

        private static ScoreDto MapToDto(Score s) => new ScoreDto
        {
            ScoreId = s.ScoreId,
            EnrollmentId = s.EnrollmentId,
            ProcessScore = s.ProcessScore,
            MidtermScore = s.MidtermScore,
            FinalScore = s.FinalScore,
            TotalScore = s.TotalScore,
            IsPassed = s.IsPassed
        };
    }
}