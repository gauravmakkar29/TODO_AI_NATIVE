using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public interface IUserService
{
    Task<ThemePreferenceResponse> GetThemePreferenceAsync(int userId);
    Task<ThemePreferenceResponse> UpdateThemePreferenceAsync(int userId, ThemePreferenceRequest request);
}

