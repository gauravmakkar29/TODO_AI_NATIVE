using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<bool> LogoutAsync(string token);
    Task<string> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(PasswordResetConfirmRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}

