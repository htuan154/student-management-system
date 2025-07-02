using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Dtos.Role;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<RoleDto?> GetByIdAsync(string roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null) return null;
            return new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description
            };
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description
            });
        }

        public async Task<bool> IsRoleIdExistsAsync(string roleId)
        {
            return await _roleRepository.IsRoleIdExistsAsync(roleId);
        }

        public async Task<bool> IsRoleNameExistsAsync(string roleName, string? excludeRoleId = null)
        {
            return await _roleRepository.IsRoleNameExistsAsync(roleName, excludeRoleId);
        }

        public async Task<IEnumerable<RoleDto>> SearchRolesAsync(string searchTerm)
        {
            var roles = await _roleRepository.SearchRolesAsync(searchTerm);
            return roles.Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description
            });
        }

        public async Task<(IEnumerable<RoleDto> Roles, int TotalCount)> GetPagedRolesAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (roles, totalCount) = await _roleRepository.GetPagedRolesAsync(pageNumber, pageSize, searchTerm);
            var dtos = roles.Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description
            });
            return (dtos, totalCount);
        }

        public async Task<bool> CreateAsync(RoleCreateDto dto)
        {
            if (await _roleRepository.IsRoleIdExistsAsync(dto.RoleId)) return false;
            var role = new Role
            {
                RoleId = dto.RoleId,
                RoleName = dto.RoleName,
                Description = dto.Description
            };
            await _roleRepository.AddAsync(role);
            return true;
        }

        public async Task<bool> UpdateAsync(RoleUpdateDto dto)
        {
            var role = await _roleRepository.GetByIdAsync(dto.RoleId);
            if (role == null) return false;
            role.RoleName = dto.RoleName;
            role.Description = dto.Description;
            await _roleRepository.UpdateAsync(role);
            return true;
        }

        public async Task<bool> DeleteAsync(string roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null) return false;
            await _roleRepository.DeleteAsync(role);
            return true;
        }
    }
}