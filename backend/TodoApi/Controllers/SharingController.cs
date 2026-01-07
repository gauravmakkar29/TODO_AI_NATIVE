using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApi.Models.DTOs;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SharingController : ControllerBase
{
    private readonly ISharingService _sharingService;

    public SharingController(ISharingService sharingService)
    {
        _sharingService = sharingService;
    }

    [HttpPost("share")]
    public async Task<ActionResult<ShareTodoResponse>> ShareTodo([FromBody] ShareTodoRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var result = await _sharingService.ShareTodoAsync(request, userId.Value);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("unshare/{todoId}/{sharedWithUserId}")]
    public async Task<ActionResult> UnshareTodo(int todoId, int sharedWithUserId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var success = await _sharingService.UnshareTodoAsync(todoId, sharedWithUserId, userId.Value);
        if (!success)
            return NotFound(new { message = "Share not found or you don't have permission to unshare" });

        return NoContent();
    }

    [HttpPut("permission/{todoId}/{sharedWithUserId}")]
    public async Task<ActionResult> UpdatePermission(int todoId, int sharedWithUserId, [FromBody] UpdateSharePermissionRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var success = await _sharingService.UpdateSharePermissionAsync(todoId, sharedWithUserId, request, userId.Value);
        if (!success)
            return NotFound(new { message = "Share not found or you don't have permission to update it" });

        return Ok(new { message = "Permission updated successfully" });
    }

    [HttpGet("todo/{todoId}")]
    public async Task<ActionResult<IEnumerable<ShareTodoResponse>>> GetTodoShares(int todoId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var shares = await _sharingService.GetTodoSharesAsync(todoId, userId.Value);
        return Ok(shares);
    }

    [HttpGet("shared")]
    public async Task<ActionResult<IEnumerable<SharedTodoDto>>> GetSharedTodos()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var todos = await _sharingService.GetSharedTodosAsync(userId.Value);
        return Ok(todos);
    }

    [HttpGet("activity/{todoId}")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetTodoActivities(int todoId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var activities = await _sharingService.GetTodoActivitiesAsync(todoId, userId.Value);
        return Ok(activities);
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

