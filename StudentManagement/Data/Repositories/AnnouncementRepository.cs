using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class AnnouncementRepository : GenericRepository<Announcement>, IAnnouncementRepository
    {
        public AnnouncementRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Announcement?> GetAnnouncementWithDetailsAsync(int announcementId)
        {
            return await _dbSet
                .Include(a => a.User)
                .Include(a => a.AnnouncementDetails)
                    .ThenInclude(ad => ad.Role)
                .Include(a => a.AnnouncementDetails)
                    .ThenInclude(ad => ad.Class)
                .Include(a => a.AnnouncementDetails)
                    .ThenInclude(ad => ad.Course)
                .Include(a => a.AnnouncementDetails)
                    .ThenInclude(ad => ad.User)
                .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);
        }

        public async Task<IEnumerable<Announcement>> GetAnnouncementsByUserAsync(string userId)
        {
            return await _dbSet
                .Include(a => a.User)
                .Where(a => a.AnnouncementDetails.Any(ad => ad.UserId == userId) ||
                           a.CreatedBy == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetAnnouncementsByRoleAsync(string roleId)
        {
            return await _dbSet
                .Include(a => a.User)
                .Where(a => a.AnnouncementDetails.Any(ad => ad.RoleId == roleId))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetAnnouncementsByClassAsync(string classId)
        {
            return await _dbSet
                .Include(a => a.User)
                .Where(a => a.AnnouncementDetails.Any(ad => ad.ClassId == classId))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetAnnouncementsByCourseAsync(string courseId)
        {
            return await _dbSet
                .Include(a => a.User)
                .Where(a => a.AnnouncementDetails.Any(ad => ad.CourseId == courseId))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync()
        {
            var now = DateTime.Now;
            return await _dbSet
                .Include(a => a.User)
                .Where(a => a.ExpiryDate == null || a.ExpiryDate > now)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetExpiredAnnouncementsAsync()
        {
            var now = DateTime.Now;
            return await _dbSet
                .Include(a => a.User)
                .Where(a => a.ExpiryDate != null && a.ExpiryDate <= now)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Announcement> Announcements, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet.Include(a => a.User).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => a.Title.Contains(searchTerm) ||
                                        a.Content.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var announcements = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (announcements, totalCount);
        }
    }
}
