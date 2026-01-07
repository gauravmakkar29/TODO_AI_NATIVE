using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoApi.Data;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<object>>> SearchUsers([FromQuery] string? email = null, [FromQuery] string? query = null)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        IQueryable<Models.User> userQuery = _context.Users;

        if (!string.IsNullOrWhiteSpace(email))
        {
            userQuery = userQuery.Where(u => u.Email.ToLower().Contains(email.ToLower()));
        }
        else if (!string.IsNullOrWhiteSpace(query))
        {
            userQuery = userQuery.Where(u => 
                u.Email.ToLower().Contains(query.ToLower()) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(query.ToLower())) ||
                (u.LastName != null && u.LastName.ToLower().Contains(query.ToLower())));
        }
        else
        {
            return BadRequest(new { message = "Email or query parameter is required" });
        }

        // Exclude current user
        userQuery = userQuery.Where(u => u.Id != userId.Value);

        var users = await userQuery
            .Select(u => new
            {
                id = u.Id,
                email = u.Email,
                firstName = u.FirstName,
                lastName = u.LastName
            })
            .Take(10)
            .ToListAsync();

        return Ok(users);
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

