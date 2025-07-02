using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> IsUsernameExistsAsync(string username, string? excludeUserId = null);
        Task<bool> IsEmailExistsAsync(string email, string? excludeUserId = null);
        Task<User?> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
        Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
    }
}