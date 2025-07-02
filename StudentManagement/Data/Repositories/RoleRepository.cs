using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsRoleIdExistsAsync(string roleId)
        {
            return await _context.Roles.AnyAsync(r => r.RoleId == roleId);
        }

        public async Task<bool> IsRoleNameExistsAsync(string roleName, string? excludeRoleId = null)
        {
            return await _context.Roles.AnyAsync(r =>
                r.RoleName == roleName &&
                (excludeRoleId == null || r.RoleId != excludeRoleId));
        }

        public async Task<IEnumerable<Role>> SearchRolesAsync(string searchTerm)
        {
            return await _context.Roles
                .Where(r => r.RoleName.Contains(searchTerm) || (r.Description != null && r.Description.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Role> Roles, int TotalCount)> GetPagedRolesAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.RoleName.Contains(searchTerm) || (r.Description != null && r.Description.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var roles = await query
                .OrderBy(r => r.RoleName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (roles, totalCount);
        }
    }
}