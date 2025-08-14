using StudentManagementSystem.DTOs.Announcement;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<AnnouncementDto>> GetAllAsync();
        Task<AnnouncementDto?> GetByIdAsync(int id);
        Task<AnnouncementDto?> GetAnnouncementWithDetailsAsync(int announcementId);
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByUserAsync(string userId);
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByRoleAsync(string roleId);
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByClassAsync(string classId);
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByCourseAsync(string courseId);
        Task<IEnumerable<AnnouncementDto>> GetActiveAnnouncementsAsync();
        Task<IEnumerable<AnnouncementDto>> GetExpiredAnnouncementsAsync();
        Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto createDto, string createdBy);
        Task<AnnouncementDto?> UpdateAsync(int id, UpdateAnnouncementDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<(IEnumerable<AnnouncementDto> Announcements, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
    }
}
