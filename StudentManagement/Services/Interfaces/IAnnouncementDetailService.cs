using StudentManagementSystem.DTOs.AnnouncementDetail;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface IAnnouncementDetailService
    {
        Task<IEnumerable<AnnouncementDetailDto>> GetAllAsync();
        Task<AnnouncementDetailDto?> GetByIdAsync(int id);
        Task<IEnumerable<AnnouncementDetailDto>> GetDetailsByAnnouncementIdAsync(int announcementId);
        Task<IEnumerable<AnnouncementDetailDto>> GetDetailsByRoleIdAsync(string roleId);
        Task<IEnumerable<AnnouncementDetailDto>> GetDetailsByClassIdAsync(string classId);
        Task<IEnumerable<AnnouncementDetailDto>> GetDetailsByCourseIdAsync(string courseId);
        Task<IEnumerable<AnnouncementDetailDto>> GetDetailsByUserIdAsync(string userId);
        Task<AnnouncementDetailDto> CreateAsync(CreateAnnouncementDetailDto createDto);
        Task<AnnouncementDetailDto?> UpdateAsync(int id, UpdateAnnouncementDetailDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteDetailsByAnnouncementIdAsync(int announcementId);
        Task<(IEnumerable<AnnouncementDetailDto> Details, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<bool> CreateBulkDetailsAsync(int announcementId, BulkCreateAnnouncementDetailDto bulkCreateDto);
        Task<IEnumerable<AnnouncementDetailDto>> GetUserAnnouncementDetailsAsync(string userId);
    }
}
