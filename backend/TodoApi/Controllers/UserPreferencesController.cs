using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApi.Models.DTOs;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserPreferencesController : ControllerBase
{
    private readonly IUserService _userService;

    public UserPreferencesController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("theme")]
    public async Task<ActionResult<ThemePreferenceResponse>> GetThemePreference()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var theme = await _userService.GetThemePreferenceAsync(userId.Value);
            return Ok(theme);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
    }

    [HttpPut("theme")]
    public async Task<ActionResult<ThemePreferenceResponse>> UpdateThemePreference([FromBody] ThemePreferenceRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (request.Theme != "light" && request.Theme != "dark")
            return BadRequest(new { message = "Theme must be 'light' or 'dark'" });

        try
        {
            var theme = await _userService.UpdateThemePreferenceAsync(userId.Value, request);
            return Ok(theme);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}


