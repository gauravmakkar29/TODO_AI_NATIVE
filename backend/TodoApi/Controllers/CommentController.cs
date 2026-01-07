using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApi.Models.DTOs;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Comment))
            return BadRequest(new { message = "Comment is required" });

        try
        {
            var comment = await _commentService.CreateCommentAsync(request, userId.Value);
            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CommentDto>> UpdateComment(int id, [FromBody] UpdateCommentRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Comment))
            return BadRequest(new { message = "Comment is required" });

        try
        {
            var comment = await _commentService.UpdateCommentAsync(id, request, userId.Value);
            if (comment == null)
                return NotFound(new { message = "Comment not found" });

            return Ok(comment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var success = await _commentService.DeleteCommentAsync(id, userId.Value);
        if (!success)
            return NotFound(new { message = "Comment not found or you don't have permission to delete it" });

        return NoContent();
    }

    [HttpGet("todo/{todoId}")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetTodoComments(int todoId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var comments = await _commentService.GetTodoCommentsAsync(todoId, userId.Value);
        return Ok(comments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDto>> GetComment(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var comment = await _commentService.GetCommentByIdAsync(id, userId.Value);
        if (comment == null)
            return NotFound(new { message = "Comment not found" });

        return Ok(comment);
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

