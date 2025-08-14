using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class AnnouncementDetailRepository : GenericRepository<AnnouncementDetail>, IAnnouncementDetailRepository
    {
        public AnnouncementDetailRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AnnouncementDetail>> GetDetailsByAnnouncementIdAsync(int announcementId)
        {
            return await _dbSet
                .Include(ad => ad.Announcement)
                .Include(ad => ad.Role)
                .Include(ad => ad.Class)
                .Include(ad => ad.Course)
                .Include(ad => ad.User)
                .Where(ad => ad.AnnouncementId == announcementId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnnouncementDetail>> GetDetailsByRoleIdAsync(string roleId)
        {
            return await _dbSet
                .Include(ad => ad.Announcement)
                .Where(ad => ad.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnnouncementDetail>> GetDetailsByClassIdAsync(string classId)
        {
            return await _dbSet
                .Include(ad => ad.Announcement)
                .Where(ad => ad.ClassId == classId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnnouncementDetail>> GetDetailsByCourseIdAsync(string courseId)
        {
            return await _dbSet
                .Include(ad => ad.Announcement)
                .Where(ad => ad.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnnouncementDetail>> GetDetailsByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(ad => ad.Announcement)
                .Where(ad => ad.UserId == userId)
                .ToListAsync();
        }

        public async Task DeleteDetailsByAnnouncementIdAsync(int announcementId)
        {
            var details = await _dbSet
                .Where(ad => ad.AnnouncementId == announcementId)
                .ToListAsync();

            _dbSet.RemoveRange(details);
            await _context.SaveChangesAsync();
        }
    }
}
