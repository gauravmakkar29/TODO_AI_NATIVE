using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ThemePreferenceResponse> GetThemePreferenceAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        return new ThemePreferenceResponse
        {
            Theme = user.ThemePreference ?? "light"
        };
    }

    public async Task<ThemePreferenceResponse> UpdateThemePreferenceAsync(int userId, ThemePreferenceRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        if (request.Theme != "light" && request.Theme != "dark")
            throw new ArgumentException("Theme must be 'light' or 'dark'");

        user.ThemePreference = request.Theme;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ThemePreferenceResponse
        {
            Theme = user.ThemePreference
        };
    }
}

