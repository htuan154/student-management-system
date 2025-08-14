using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Announcement;
using StudentManagementSystem.DTOs.AnnouncementDetail;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly IAnnouncementDetailRepository _announcementDetailRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AnnouncementService> _logger;

        public AnnouncementService(
            IAnnouncementRepository announcementRepository,
            IAnnouncementDetailRepository announcementDetailRepository,
            ICacheService cacheService,
            ILogger<AnnouncementService> logger)
        {
            _announcementRepository = announcementRepository;
            _announcementDetailRepository = announcementDetailRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAllAsync()
        {
            var announcements = await _announcementRepository.GetAllAsync();
            return announcements.Select(MapToDto);
        }

        public async Task<AnnouncementDto?> GetByIdAsync(int id)
        {
            var announcement = await _announcementRepository.GetByIdAsync(id);
            return announcement == null ? null : MapToDto(announcement);
        }

        public async Task<AnnouncementDto?> GetAnnouncementWithDetailsAsync(int announcementId)
        {
            var announcement = await _announcementRepository.GetAnnouncementWithDetailsAsync(announcementId);
            return announcement == null ? null : MapToDto(announcement);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByUserAsync(string userId)
        {
            var announcements = await _announcementRepository.GetAnnouncementsByUserAsync(userId);
            return announcements.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByRoleAsync(string roleId)
        {
            var announcements = await _announcementRepository.GetAnnouncementsByRoleAsync(roleId);
            return announcements.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByClassAsync(string classId)
        {
            var announcements = await _announcementRepository.GetAnnouncementsByClassAsync(classId);
            return announcements.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByCourseAsync(string courseId)
        {
            var announcements = await _announcementRepository.GetAnnouncementsByCourseAsync(courseId);
            return announcements.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetActiveAnnouncementsAsync()
        {
            var announcements = await _announcementRepository.GetActiveAnnouncementsAsync();
            return announcements.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetExpiredAnnouncementsAsync()
        {
            var announcements = await _announcementRepository.GetExpiredAnnouncementsAsync();
            return announcements.Select(MapToDto);
        }

        public async Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto createDto, string createdBy)
        {
            var announcement = new Announcement
            {
                Title = createDto.Title,
                Content = createDto.Content,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now,
                ExpiryDate = createDto.ExpiryDate
            };

            var createdAnnouncement = await _announcementRepository.AddAsync(announcement);

            // Create announcement details
            if (createDto.RoleIds?.Any() == true)
            {
                foreach (var roleId in createDto.RoleIds)
                {
                    await _announcementDetailRepository.AddAsync(new AnnouncementDetail
                    {
                        AnnouncementId = createdAnnouncement.AnnouncementId,
                        RoleId = roleId
                    });
                }
            }

            if (createDto.ClassIds?.Any() == true)
            {
                foreach (var classId in createDto.ClassIds)
                {
                    await _announcementDetailRepository.AddAsync(new AnnouncementDetail
                    {
                        AnnouncementId = createdAnnouncement.AnnouncementId,
                        ClassId = classId
                    });
                }
            }

            if (createDto.CourseIds?.Any() == true)
            {
                foreach (var courseId in createDto.CourseIds)
                {
                    await _announcementDetailRepository.AddAsync(new AnnouncementDetail
                    {
                        AnnouncementId = createdAnnouncement.AnnouncementId,
                        CourseId = courseId
                    });
                }
            }

            if (createDto.UserIds?.Any() == true)
            {
                foreach (var userId in createDto.UserIds)
                {
                    await _announcementDetailRepository.AddAsync(new AnnouncementDetail
                    {
                        AnnouncementId = createdAnnouncement.AnnouncementId,
                        UserId = userId
                    });
                }
            }

            _logger.LogInformation("Created announcement with ID {AnnouncementId}.", createdAnnouncement.AnnouncementId);
            return MapToDto(createdAnnouncement);
        }

        public async Task<AnnouncementDto?> UpdateAsync(int id, UpdateAnnouncementDto updateDto)
        {
            var announcement = await _announcementRepository.GetByIdAsync(id);
            if (announcement == null) return null;

            announcement.Title = updateDto.Title;
            announcement.Content = updateDto.Content;
            announcement.ExpiryDate = updateDto.ExpiryDate;

            await _announcementRepository.UpdateAsync(announcement);

            // Update announcement details
            await _announcementDetailRepository.DeleteDetailsByAnnouncementIdAsync(id);

            // Add new details (same logic as Create)
            // ... (tương tự như trong CreateAsync)

            _logger.LogInformation("Updated announcement with ID {AnnouncementId}.", id);
            return MapToDto(announcement);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _announcementRepository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Deleted announcement with ID {AnnouncementId}.", id);
            }
            return result;
        }

        public async Task<(IEnumerable<AnnouncementDto> Announcements, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (announcements, totalCount) = await _announcementRepository.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return (announcements.Select(MapToDto), totalCount);
        }

        private static AnnouncementDto MapToDto(Announcement announcement)
        {
            return new AnnouncementDto
            {
                AnnouncementId = announcement.AnnouncementId,
                Title = announcement.Title,
                Content = announcement.Content,
                CreatedBy = announcement.CreatedBy,
                CreatedAt = announcement.CreatedAt,
                ExpiryDate = announcement.ExpiryDate
            };
        }
    }
}
