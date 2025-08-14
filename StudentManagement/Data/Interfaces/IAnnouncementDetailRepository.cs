using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IAnnouncementDetailRepository : IRepository<AnnouncementDetail>
    {
        Task<IEnumerable<AnnouncementDetail>> GetDetailsByAnnouncementIdAsync(int announcementId);
        Task<IEnumerable<AnnouncementDetail>> GetDetailsByRoleIdAsync(string roleId);
        Task<IEnumerable<AnnouncementDetail>> GetDetailsByClassIdAsync(string classId);
        Task<IEnumerable<AnnouncementDetail>> GetDetailsByCourseIdAsync(string courseId);
        Task<IEnumerable<AnnouncementDetail>> GetDetailsByUserIdAsync(string userId);
        Task DeleteDetailsByAnnouncementIdAsync(int announcementId);
    }
}
