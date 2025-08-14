using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IAnnouncementRepository : IRepository<Announcement>
    {
        Task<Announcement?> GetAnnouncementWithDetailsAsync(int announcementId);
        Task<IEnumerable<Announcement>> GetAnnouncementsByUserAsync(string userId);
        Task<IEnumerable<Announcement>> GetAnnouncementsByRoleAsync(string roleId);
        Task<IEnumerable<Announcement>> GetAnnouncementsByClassAsync(string classId);
        Task<IEnumerable<Announcement>> GetAnnouncementsByCourseAsync(string courseId);
        Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetExpiredAnnouncementsAsync();
        Task<(IEnumerable<Announcement> Announcements, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
    }
}
