using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<bool> IsRoleIdExistsAsync(string roleId);
        Task<bool> IsRoleNameExistsAsync(string roleName, string? excludeRoleId = null);
        Task<IEnumerable<Role>> SearchRolesAsync(string searchTerm);
        Task<(IEnumerable<Role> Roles, int TotalCount)> GetPagedRolesAsync(int pageNumber, int pageSize, string? searchTerm = null);
    }
}