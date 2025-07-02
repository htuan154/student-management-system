using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Dtos.User;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace StudentManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto?> GetByIdAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user == null ? null : MapToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }

        public async Task<bool> IsUsernameExistsAsync(string username, string? excludeUserId = null)
        {
            return await _userRepository.IsUsernameExistsAsync(username, excludeUserId);
        }

        public async Task<bool> IsEmailExistsAsync(string email, string? excludeUserId = null)
        {
            return await _userRepository.IsEmailExistsAsync(email, excludeUserId);
        }

        public async Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm)
        {
            var users = await _userRepository.SearchUsersAsync(searchTerm);
            return users.Select(MapToDto);
        }

        public async Task<(IEnumerable<UserDto> Users, int TotalCount)> GetPagedUsersAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            var (users, totalCount) = await _userRepository.GetPagedUsersAsync(pageNumber, pageSize, searchTerm, isActive);
            return (users.Select(MapToDto), totalCount);
        }

        public async Task<bool> CreateAsync(UserCreateDto dto)
        {
            if (await _userRepository.IsUsernameExistsAsync(dto.Username)) return false;
            if (await _userRepository.IsEmailExistsAsync(dto.Email)) return false;

            var user = new User
            {
                UserId = dto.UserId,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password), // Hash password tại đây!
                RoleId = dto.RoleId,
                StudentId = dto.StudentId,
                EmployeeId = dto.EmployeeId,
                TeacherId = dto.TeacherId,
                IsActive = dto.IsActive
            };
            await _userRepository.AddAsync(user);
            return true;
        }

        public async Task<bool> UpdateAsync(UserUpdateDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null) return false;
            user.Username = dto.Username;
            user.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = HashPassword(dto.Password);
            user.RoleId = dto.RoleId;
            user.StudentId = dto.StudentId;
            user.EmployeeId = dto.EmployeeId;
            user.TeacherId = dto.TeacherId;
            user.IsActive = dto.IsActive;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;
            await _userRepository.DeleteAsync(user);
            return true;
        }

        public async Task<bool> CheckPasswordAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null) return false;
            return user.PasswordHash == HashPassword(password);
        }

        public async Task UpdateRefreshTokenAsync(string userId, string refreshToken, DateTime expiryTime)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryTime;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task<UserDto?> GetByRefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            return user == null ? null : MapToDto(user);
        }

        private static UserDto MapToDto(User u) => new UserDto
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email,
            RoleId = u.RoleId,
            StudentId = u.StudentId,
            EmployeeId = u.EmployeeId,
            TeacherId = u.TeacherId,
            IsActive = u.IsActive,
            RefreshTokenExpiryTime = u.RefreshTokenExpiryTime
        };

        // Hàm hash password (ví dụ dùng SHA256, hoặc tốt hơn là BCrypt)
        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
