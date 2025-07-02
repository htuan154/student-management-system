using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class ScoreRepository : GenericRepository<Score>, IScoreRepository
    {
        private readonly ApplicationDbContext _context;

        public ScoreRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Score?> GetByEnrollmentIdAsync(int enrollmentId)
        {
            return await _context.Scores.FirstOrDefaultAsync(s => s.EnrollmentId == enrollmentId);
        }

        public async Task<IEnumerable<Score>> SearchScoresAsync(string searchTerm)
        {
            return await _context.Scores
                .Where(s => s.EnrollmentId.ToString().Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Score> Scores, int TotalCount)> GetPagedScoresAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.Scores.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.EnrollmentId.ToString().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var scores = await query
                .OrderBy(s => s.ScoreId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (scores, totalCount);
        }
    }
}