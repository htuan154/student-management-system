using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Dtos.Role;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<RoleService> _logger;
        private const string CachePrefix = "role";
        private const string AllRolesCacheKey = "roles:all";

        public RoleService(IRoleRepository roleRepository, ICacheService cacheService, ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<RoleDto?> GetByIdAsync(string roleId)
        {
            _logger.LogInformation("Getting role by ID: {RoleId}", roleId);
            var cacheKey = $"{CachePrefix}:{roleId}";

            var cachedData = await _cacheService.GetDataAsync<RoleDto>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Role {RoleId} found in cache.", roleId);
                return cachedData;
            }

            _logger.LogInformation("Role {RoleId} not found in cache. Fetching from database.", roleId);
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null) return null;

            var dto = MapToDto(role);
            // Roles change infrequently, so we can use a longer cache duration
            await _cacheService.SetDataAsync(cacheKey, dto, System.DateTimeOffset.Now.AddHours(1));
            return dto;
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            _logger.LogInformation("Getting all roles.");

            var cachedData = await _cacheService.GetDataAsync<IEnumerable<RoleDto>>(AllRolesCacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("All roles retrieved from cache.");
                return cachedData;
            }

            _logger.LogInformation("All roles not in cache. Fetching from database.");
            var roles = await _roleRepository.GetAllAsync();
            var dtos = roles.Select(MapToDto).ToList();

            await _cacheService.SetDataAsync(AllRolesCacheKey, dtos, System.DateTimeOffset.Now.AddHours(1));
            return dtos;
        }

        public async Task<bool> CreateAsync(RoleCreateDto dto)
        {
            _logger.LogInformation("Creating new role with ID: {RoleId}", dto.RoleId);
            if (await _roleRepository.IsRoleIdExistsAsync(dto.RoleId))
            {
                _logger.LogWarning("Create failed. Role ID {RoleId} already exists.", dto.RoleId);
                return false;
            }

            var role = new Role { RoleId = dto.RoleId, RoleName = dto.RoleName, Description = dto.Description };
            await _roleRepository.AddAsync(role);

            // Invalidate the cache for the list of all roles
            await _cacheService.RemoveDataAsync(AllRolesCacheKey);
            _logger.LogInformation("Role {RoleId} created successfully. 'All Roles' cache invalidated.", dto.RoleId);

            return true;
        }

        public async Task<bool> UpdateAsync(RoleUpdateDto dto)
        {
            _logger.LogInformation("Updating role with ID: {RoleId}", dto.RoleId);
            var role = await _roleRepository.GetByIdAsync(dto.RoleId);
            if (role == null)
            {
                _logger.LogWarning("Update failed. Role {RoleId} not found.", dto.RoleId);
                return false;
            }

            role.RoleName = dto.RoleName;
            role.Description = dto.Description;
            await _roleRepository.UpdateAsync(role);

            // Invalidate both the specific role cache and the full list cache
            await _cacheService.RemoveDataAsync($"{CachePrefix}:{dto.RoleId}");
            await _cacheService.RemoveDataAsync(AllRolesCacheKey);
            _logger.LogInformation("Role {RoleId} updated successfully. Caches invalidated.", dto.RoleId);

            return true;
        }

        public async Task<bool> DeleteAsync(string roleId)
        {
            _logger.LogInformation("Deleting role with ID: {RoleId}", roleId);
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                _logger.LogWarning("Delete failed. Role {RoleId} not found.", roleId);
                return false;
            }

            await _roleRepository.DeleteAsync(role);

            // Invalidate both the specific role cache and the full list cache
            await _cacheService.RemoveDataAsync($"{CachePrefix}:{roleId}");
            await _cacheService.RemoveDataAsync(AllRolesCacheKey);
            _logger.LogInformation("Role {RoleId} deleted successfully. Caches invalidated.", roleId);

            return true;
        }

        // ... Other methods with logging
        public Task<bool> IsRoleIdExistsAsync(string roleId) => _roleRepository.IsRoleIdExistsAsync(roleId);
        public Task<bool> IsRoleNameExistsAsync(string roleName, string? excludeRoleId = null) => _roleRepository.IsRoleNameExistsAsync(roleName, excludeRoleId);

        public async Task<IEnumerable<RoleDto>> SearchRolesAsync(string searchTerm)
        {
            _logger.LogInformation("Searching for roles with term: {SearchTerm}", searchTerm);
            var roles = await _roleRepository.SearchRolesAsync(searchTerm);
            return roles.Select(MapToDto);
        }

        public async Task<(IEnumerable<RoleDto> Roles, int TotalCount)> GetPagedRolesAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            _logger.LogInformation("Getting paged roles. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            var (roles, totalCount) = await _roleRepository.GetPagedRolesAsync(pageNumber, pageSize, searchTerm);
            return (roles.Select(MapToDto), totalCount);
        }

        private static RoleDto MapToDto(Role role) => new RoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description
        };
    }
}
