using StudentManagementSystem.DTOs.Auth;

public interface IAuthService
{
    Task<(string accessToken, string refreshToken)> LoginAsync(LoginDto dto);
    Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken);
}
