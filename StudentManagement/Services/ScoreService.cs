using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Dtos.Score;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public class ScoreService : IScoreService
    {
        private readonly IScoreRepository _scoreRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ScoreService> _logger;
        private const string CachePrefix = "score";

        public ScoreService(IScoreRepository scoreRepository, ICacheService cacheService, ILogger<ScoreService> logger)
        {
            _scoreRepository = scoreRepository;
            _cacheService = cacheService;
            _logger = logger;
        }
            public async Task<IEnumerable<ScoreDto>> GetAllAsync()
            {
                _logger.LogInformation("Fetching all scores from repository.");
                var scores = await _scoreRepository.GetAllAsync();
                return scores.Select(MapToDto);
            }
        public async Task<ScoreDto?> GetByIdAsync(int scoreId)
        {
            _logger.LogInformation("Getting score by ID: {ScoreId}", scoreId);
            var cacheKey = $"{CachePrefix}:{scoreId}";

            var cachedData = await _cacheService.GetDataAsync<ScoreDto>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Score {ScoreId} found in cache.", scoreId);
                return cachedData;
            }

            var score = await _scoreRepository.GetByIdAsync(scoreId);
            if (score == null) return null;

            var dto = MapToDto(score);
            await _cacheService.SetDataAsync(cacheKey, dto, System.DateTimeOffset.Now.AddMinutes(5));
            return dto;
        }

        public async Task<ScoreDto?> GetByEnrollmentIdAsync(int enrollmentId)
        {
            _logger.LogInformation("Getting score by Enrollment ID: {EnrollmentId}", enrollmentId);
            var cacheKey = $"{CachePrefix}:enrollment:{enrollmentId}";

            var cachedData = await _cacheService.GetDataAsync<ScoreDto>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Score for Enrollment ID {EnrollmentId} found in cache.", enrollmentId);
                return cachedData;
            }

            var score = await _scoreRepository.GetByEnrollmentIdAsync(enrollmentId);
            if (score == null) return null;

            var dto = MapToDto(score);
            await _cacheService.SetDataAsync(cacheKey, dto, System.DateTimeOffset.Now.AddMinutes(5));
            return dto;
        }

        public async Task<bool> CreateAsync(ScoreCreateDto dto)
        {
            _logger.LogInformation("Creating new score for Enrollment ID: {EnrollmentId}", dto.EnrollmentId);

            var score = new Score
            {
                EnrollmentId = dto.EnrollmentId,
                ProcessScore = dto.ProcessScore,
                MidtermScore = dto.MidtermScore,
                FinalScore = dto.FinalScore
            };

            await _scoreRepository.AddAsync(score);
            _logger.LogInformation("Score created for Enrollment ID {EnrollmentId}. Total score will be computed by the database.", dto.EnrollmentId);

            await InvalidateScoreCache(score.ScoreId, score.EnrollmentId);
            return true;
        }

        public async Task<bool> UpdateAsync(ScoreUpdateDto dto)
        {
            _logger.LogInformation("Updating score with ID: {ScoreId}", dto.ScoreId);
            var score = await _scoreRepository.GetByIdAsync(dto.ScoreId);
            if (score == null)
            {
                _logger.LogWarning("Update failed. Score with ID {ScoreId} not found.", dto.ScoreId);
                return false;
            }

            // Only update the raw scores. The database will handle the rest.
            score.ProcessScore = dto.ProcessScore;
            score.MidtermScore = dto.MidtermScore;
            score.FinalScore = dto.FinalScore;

            await _scoreRepository.UpdateAsync(score);
            _logger.LogInformation("Score {ScoreId} updated. Total score will be re-computed by the database.", dto.ScoreId);

            await InvalidateScoreCache(score.ScoreId, score.EnrollmentId);
            return true;
        }

        public async Task<bool> DeleteAsync(int scoreId)
        {
            _logger.LogInformation("Deleting score with ID: {ScoreId}", scoreId);
            var score = await _scoreRepository.GetByIdAsync(scoreId);
            if (score == null)
            {
                _logger.LogWarning("Delete failed. Score with ID {ScoreId} not found.", scoreId);
                return false;
            }

            await _scoreRepository.DeleteAsync(score);
            _logger.LogInformation("Score {ScoreId} deleted successfully.", scoreId);

            await InvalidateScoreCache(scoreId, score.EnrollmentId);
            return true;
        }

        private async Task InvalidateScoreCache(int scoreId, int enrollmentId)
        {
            var scoreCacheKey = $"{CachePrefix}:{scoreId}";
            var enrollmentCacheKey = $"{CachePrefix}:enrollment:{enrollmentId}";

            await _cacheService.RemoveDataAsync(scoreCacheKey);
            await _cacheService.RemoveDataAsync(enrollmentCacheKey);

            _logger.LogInformation("Caches invalidated for ScoreId {ScoreId} and EnrollmentId {EnrollmentId}", scoreId, enrollmentId);
        }

        // Other Methods
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
