using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.AnnouncementDetail;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class AnnouncementDetailService : IAnnouncementDetailService
    {
        private readonly IAnnouncementDetailRepository _announcementDetailRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AnnouncementDetailService> _logger;

        public AnnouncementDetailService(
            IAnnouncementDetailRepository announcementDetailRepository,
            ICacheService cacheService,
            ILogger<AnnouncementDetailService> logger)
        {
            _announcementDetailRepository = announcementDetailRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<AnnouncementDetailDto>> GetAllAsync()
        {
            var details = await _announcementDetailRepository.GetAllAsync();
            return details.Select(MapToDto);
        }

        public async Task<AnnouncementDetailDto?> GetByIdAsync(int id)
        {
            var detail = await _announcementDetailRepository.GetByIdAsync(id);
            return detail == null ? null : MapToDto(detail);
        }

        public async Task<IEnumerable<AnnouncementDetailDto>> GetDetailsByAnnouncementIdAsync(int announcementId)
        {
            var details = await _announcementDetailRepository.GetDetailsByAnnouncementIdAsync(announcementId);
            return details.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDetailDto>> GetDetailsByRoleIdAsync(string roleId)
        {
            var details = await _announcementDetailRepository.GetDetailsByRoleIdAsync(roleId);
            return details.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDetailDto>> GetDetailsByClassIdAsync(string classId)
        {
            var details = await _announcementDetailRepository.GetDetailsByClassIdAsync(classId);
            return details.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDetailDto>> GetDetailsByCourseIdAsync(string courseId)
        {
            var details = await _announcementDetailRepository.GetDetailsByCourseIdAsync(courseId);
            return details.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDetailDto>> GetDetailsByUserIdAsync(string userId)
        {
            var details = await _announcementDetailRepository.GetDetailsByUserIdAsync(userId);
            return details.Select(MapToDto);
        }

        public async Task<AnnouncementDetailDto> CreateAsync(CreateAnnouncementDetailDto createDto)
        {
            // Validation - At least one target must be specified
            if (string.IsNullOrEmpty(createDto.RoleId) &&
                string.IsNullOrEmpty(createDto.ClassId) &&
                string.IsNullOrEmpty(createDto.CourseId) &&
                string.IsNullOrEmpty(createDto.UserId))
            {
                throw new ArgumentException("At least one target (Role, Class, Course, or User) must be specified.");
            }

            var detail = new AnnouncementDetail
            {
                AnnouncementId = createDto.AnnouncementId,
                RoleId = createDto.RoleId,
                ClassId = createDto.ClassId,
                CourseId = createDto.CourseId,
                UserId = createDto.UserId
            };

            var createdDetail = await _announcementDetailRepository.AddAsync(detail);

            // Clear cache
            await _cacheService.RemoveByPatternAsync("announcement");

            _logger.LogInformation("Created AnnouncementDetail with ID {DetailId} for Announcement {AnnouncementId}.",
                createdDetail.AnnouncementDetailId, createdDetail.AnnouncementId);

            return MapToDto(createdDetail);
        }

        public async Task<AnnouncementDetailDto?> UpdateAsync(int id, UpdateAnnouncementDetailDto updateDto)
        {
            var detail = await _announcementDetailRepository.GetByIdAsync(id);
            if (detail == null) return null;

            // Validation - At least one target must be specified
            if (string.IsNullOrEmpty(updateDto.RoleId) &&
                string.IsNullOrEmpty(updateDto.ClassId) &&
                string.IsNullOrEmpty(updateDto.CourseId) &&
                string.IsNullOrEmpty(updateDto.UserId))
            {
                throw new ArgumentException("At least one target (Role, Class, Course, or User) must be specified.");
            }

            detail.RoleId = updateDto.RoleId;
            detail.ClassId = updateDto.ClassId;
            detail.CourseId = updateDto.CourseId;
            detail.UserId = updateDto.UserId;

            await _announcementDetailRepository.UpdateAsync(detail);

            // Clear cache
            await _cacheService.RemoveByPatternAsync("announcement");

            _logger.LogInformation("Updated AnnouncementDetail with ID {DetailId}.", id);
            return MapToDto(detail);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _announcementDetailRepository.DeleteAsync(id);

            if (result)
            {
                // Clear cache
                await _cacheService.RemoveByPatternAsync("announcement");
                _logger.LogInformation("Deleted AnnouncementDetail with ID {DetailId}.", id);
            }

            return result;
        }

        public async Task<bool> DeleteDetailsByAnnouncementIdAsync(int announcementId)
        {
            try
            {
                await _announcementDetailRepository.DeleteDetailsByAnnouncementIdAsync(announcementId);

                // Clear cache
                await _cacheService.RemoveByPatternAsync("announcement");

                _logger.LogInformation("Deleted all AnnouncementDetails for Announcement {AnnouncementId}.", announcementId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting AnnouncementDetails for Announcement {AnnouncementId}.", announcementId);
                return false;
            }
        }

        public async Task<(IEnumerable<AnnouncementDetailDto> Details, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            // Implementation depends on repository having GetPagedAsync method
            // For now, return all and apply paging in memory (not ideal for production)
            var allDetails = await _announcementDetailRepository.GetAllAsync();

            var query = allDetails.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d =>
                    (d.RoleId != null && d.RoleId.Contains(searchTerm)) ||
                    (d.ClassId != null && d.ClassId.Contains(searchTerm)) ||
                    (d.CourseId != null && d.CourseId.Contains(searchTerm)) ||
                    (d.UserId != null && d.UserId.Contains(searchTerm)));
            }

            var totalCount = query.Count();
            var details = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            return (details, totalCount);
        }

        public async Task<bool> CreateBulkDetailsAsync(int announcementId, BulkCreateAnnouncementDetailDto bulkCreateDto)
        {
            try
            {
                var details = new List<AnnouncementDetail>();

                // Add role-based details
                if (bulkCreateDto.RoleIds?.Any() == true)
                {
                    details.AddRange(bulkCreateDto.RoleIds.Select(roleId => new AnnouncementDetail
                    {
                        AnnouncementId = announcementId,
                        RoleId = roleId
                    }));
                }

                // Add class-based details
                if (bulkCreateDto.ClassIds?.Any() == true)
                {
                    details.AddRange(bulkCreateDto.ClassIds.Select(classId => new AnnouncementDetail
                    {
                        AnnouncementId = announcementId,
                        ClassId = classId
                    }));
                }

                // Add course-based details
                if (bulkCreateDto.CourseIds?.Any() == true)
                {
                    details.AddRange(bulkCreateDto.CourseIds.Select(courseId => new AnnouncementDetail
                    {
                        AnnouncementId = announcementId,
                        CourseId = courseId
                    }));
                }

                // Add user-based details
                if (bulkCreateDto.UserIds?.Any() == true)
                {
                    details.AddRange(bulkCreateDto.UserIds.Select(userId => new AnnouncementDetail
                    {
                        AnnouncementId = announcementId,
                        UserId = userId
                    }));
                }

                // Bulk insert
                foreach (var detail in details)
                {
                    await _announcementDetailRepository.AddAsync(detail);
                }

                // Clear cache
                await _cacheService.RemoveByPatternAsync("announcement");

                _logger.LogInformation("Created {Count} AnnouncementDetails for Announcement {AnnouncementId}.",
                    details.Count, announcementId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk AnnouncementDetails for Announcement {AnnouncementId}.", announcementId);
                return false;
            }
        }

        public async Task<IEnumerable<AnnouncementDetailDto>> GetUserAnnouncementDetailsAsync(string userId)
        {
            // Get details that target this specific user
            var userSpecificDetails = await _announcementDetailRepository.GetDetailsByUserIdAsync(userId);

            // Note: In a real implementation, you might also want to get details based on user's role, class, enrolled courses
            // This would require additional repository methods and user context

            return userSpecificDetails.Select(MapToDto);
        }

        private static AnnouncementDetailDto MapToDto(AnnouncementDetail detail)
        {
            return new AnnouncementDetailDto
            {
                AnnouncementDetailId = detail.AnnouncementDetailId,
                AnnouncementId = detail.AnnouncementId,
                RoleId = detail.RoleId,
                ClassId = detail.ClassId,
                CourseId = detail.CourseId,
                UserId = detail.UserId,
                // Navigation properties (if loaded)
                RoleName = detail.Role?.RoleName,
                ClassName = detail.Class?.ClassName,
                CourseName = detail.Course?.CourseName,
                UserName = detail.User?.Username
            };
        }
    }
}
