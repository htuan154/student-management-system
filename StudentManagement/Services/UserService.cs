using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.User;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BCrypt.Net;

namespace StudentManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UserService> _logger;
        private const string CachePrefix = "user";

        public UserService(IUserRepository userRepository, ICacheService cacheService, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<bool> CreateAsync(UserCreateDto dto)
        {
            _logger.LogInformation("Creating new user with username: {Username}", dto.Username);
            if (await _userRepository.IsUsernameExistsAsync(dto.Username) || await _userRepository.IsEmailExistsAsync(dto.Email))
            {
                throw new System.InvalidOperationException("Username or Email already exists.");
            }

            var user = new User
            {
                UserId = dto.UserId,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),

                RoleId = dto.RoleId,
                StudentId = dto.StudentId,
                EmployeeId = dto.EmployeeId,
                TeacherId = dto.TeacherId,
                IsActive = dto.IsActive
            };
            await _userRepository.AddAsync(user);
            _logger.LogInformation("User {Username} created successfully.", dto.Username);
            return true;
        }

        public async Task<bool> UpdateAsync(UserUpdateDto dto)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", dto.UserId);
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                _logger.LogWarning("Update failed. User {UserId} not found.", dto.UserId);
                return false;
            }

            user.Username = dto.Username;
            user.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.Password))
            {

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                _logger.LogInformation("Password for user {UserId} has been updated.", dto.UserId);
            }
            user.RoleId = dto.RoleId;
            user.StudentId = dto.StudentId;
            user.EmployeeId = dto.EmployeeId;
            user.TeacherId = dto.TeacherId;
            user.IsActive = dto.IsActive;

            await _userRepository.UpdateAsync(user);

            await InvalidateUserCache(user.UserId, user.Username);
            _logger.LogInformation("User {UserId} updated successfully and cache invalidated.", dto.UserId);
            return true;
        }

        public async Task<bool> CheckPasswordAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null) return false;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public async Task UpdateRefreshTokenAsync(string userId, string refreshToken, System.DateTime expiryTime)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryTime;
                await _userRepository.UpdateAsync(user);

                await InvalidateUserCache(user.UserId, user.Username);
                _logger.LogInformation("Refresh token updated for user {UserId} and cache invalidated.", userId);
            }
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", userId);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            var username = user.Username; // Lưu lại username trước khi xóa
            await _userRepository.DeleteAsync(user);

            await InvalidateUserCache(userId, username);
            _logger.LogInformation("User {UserId} deleted successfully and cache invalidated.", userId);
            return true;
        }

        #region Các phương thức không thay đổi
        public async Task<UserDto?> GetByIdAsync(string userId)
        {
            _logger.LogInformation("Getting user by ID: {UserId}", userId);
            var cacheKey = $"{CachePrefix}:{userId}";
            var cachedData = await _cacheService.GetDataAsync<UserDto>(cacheKey);
            if (cachedData != null) return cachedData;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            var dto = MapToDto(user);
            await _cacheService.SetDataAsync(cacheKey, dto, System.DateTimeOffset.Now.AddMinutes(10));
            return dto;
        }

        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            _logger.LogInformation("Getting user by username: {Username}", username);
            var cacheKey = $"{CachePrefix}:username:{username}";
            var cachedData = await _cacheService.GetDataAsync<UserDto>(cacheKey);
            if (cachedData != null) return cachedData;

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null) return null;

            var dto = MapToDto(user);
            await _cacheService.SetDataAsync(cacheKey, dto, System.DateTimeOffset.Now.AddMinutes(10));
            return dto;
        }

        public async Task<UserDto?> GetByRefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            return user == null ? null : MapToDto(user);
        }

        private async Task InvalidateUserCache(string userId, string username)
        {
            var idKey = $"{CachePrefix}:{userId}";
            var usernameKey = $"{CachePrefix}:username:{username}";
            await _cacheService.RemoveDataAsync(idKey);
            await _cacheService.RemoveDataAsync(usernameKey);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync() => (await _userRepository.GetAllAsync()).Select(MapToDto);
        public Task<bool> IsUsernameExistsAsync(string username, string? excludeUserId = null) => _userRepository.IsUsernameExistsAsync(username, excludeUserId);
        public Task<bool> IsEmailExistsAsync(string email, string? excludeUserId = null) => _userRepository.IsEmailExistsAsync(email, excludeUserId);
        public async Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm) => (await _userRepository.SearchUsersAsync(searchTerm)).Select(MapToDto);
        public async Task<(IEnumerable<UserDto> Users, int TotalCount)> GetPagedUsersAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            var (users, totalCount) = await _userRepository.GetPagedUsersAsync(pageNumber, pageSize, searchTerm, isActive);
            return (users.Select(MapToDto), totalCount);
        }

        private static UserDto MapToDto(User u) => new UserDto
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email,
            RoleId = u.RoleId,
            RoleName = u.Role?.RoleName ?? string.Empty,
            StudentId = u.StudentId,
            EmployeeId = u.EmployeeId,
            TeacherId = u.TeacherId,
            IsActive = u.IsActive,
            RefreshTokenExpiryTime = u.RefreshTokenExpiryTime

        };
        #endregion
    }
}
