using StudentManagementSystem.DTOs.User;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(string userId);
        Task<UserDto?> GetByUsernameAsync(string username);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<bool> IsUsernameExistsAsync(string username, string? excludeUserId = null);
        Task<bool> IsEmailExistsAsync(string email, string? excludeUserId = null);
        Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm);
        Task<(IEnumerable<UserDto> Users, int TotalCount)> GetPagedUsersAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<bool> CreateAsync(UserCreateDto dto);
        Task<bool> UpdateAsync(UserUpdateDto dto);
        Task<bool> DeleteAsync(string userId);
        Task<bool> CheckPasswordAsync(string username, string password);
        Task UpdateRefreshTokenAsync(string userId, string refreshToken, DateTime expiryTime);
        Task<UserDto?> GetByRefreshTokenAsync(string refreshToken);
        Task<(bool Success, string? Error)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}
