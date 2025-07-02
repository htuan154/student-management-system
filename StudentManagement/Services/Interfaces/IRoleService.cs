using StudentManagementSystem.Dtos.Role;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface IRoleService
    {
        Task<RoleDto?> GetByIdAsync(string roleId);
        Task<IEnumerable<RoleDto>> GetAllAsync();
        Task<bool> IsRoleIdExistsAsync(string roleId);
        Task<bool> IsRoleNameExistsAsync(string roleName, string? excludeRoleId = null);
        Task<IEnumerable<RoleDto>> SearchRolesAsync(string searchTerm);
        Task<(IEnumerable<RoleDto> Roles, int TotalCount)> GetPagedRolesAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<bool> CreateAsync(RoleCreateDto dto);
        Task<bool> UpdateAsync(RoleUpdateDto dto);
        Task<bool> DeleteAsync(string roleId);
    }
}