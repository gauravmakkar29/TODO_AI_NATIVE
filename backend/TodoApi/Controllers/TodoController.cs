using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoDto>>> GetTodos([FromQuery] string? sortBy = null, [FromQuery] int? priorityFilter = null)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var todos = await _todoService.GetTodosByUserIdAsync(userId.Value, sortBy, priorityFilter);
        return Ok(todos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoDto>> GetTodo(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var todo = await _todoService.GetTodoByIdAsync(id, userId.Value);
        if (todo == null)
            return NotFound(new { message = "Todo not found" });

        return Ok(todo);
    }

    [HttpPost]
    public async Task<ActionResult<TodoDto>> CreateTodo([FromBody] CreateTodoRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title is required" });

        var todo = await _todoService.CreateTodoAsync(request, userId.Value);
        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TodoDto>> UpdateTodo(int id, [FromBody] UpdateTodoRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var todo = await _todoService.UpdateTodoAsync(id, request, userId.Value);
        if (todo == null)
            return NotFound(new { message = "Todo not found" });

        return Ok(todo);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTodo(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var deleted = await _todoService.DeleteTodoAsync(id, userId.Value);
        if (!deleted)
            return NotFound(new { message = "Todo not found" });

        return NoContent();
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<TodoDto>>> GetTodosByCategory(int categoryId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var todos = await _todoService.GetTodosByCategoryAsync(userId.Value, categoryId);
        return Ok(todos);
    }

    [HttpGet("tag/{tagId}")]
    public async Task<ActionResult<IEnumerable<TodoDto>>> GetTodosByTag(int tagId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var todos = await _todoService.GetTodosByTagAsync(userId.Value, tagId);
        return Ok(todos);
    }

    [HttpPost("search")]
    public async Task<ActionResult<SearchFilterResponse>> SearchAndFilterTodos([FromBody] SearchFilterRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _todoService.SearchAndFilterTodosAsync(userId.Value, request);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<SearchFilterResponse>> SearchAndFilterTodosGet(
        [FromQuery] string? searchQuery,
        [FromQuery] bool? isCompleted,
        [FromQuery] bool? isArchived,
        [FromQuery] int? status,
        [FromQuery] bool? isOverdue,
        [FromQuery] bool? hideCompleted,
        [FromQuery] int? priority,
        [FromQuery] List<int>? categoryIds,
        [FromQuery] List<int>? tagIds,
        [FromQuery] DateTime? dueDateFrom,
        [FromQuery] DateTime? dueDateTo,
        [FromQuery] DateTime? createdAtFrom,
        [FromQuery] DateTime? createdAtTo,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var request = new SearchFilterRequest
        {
            SearchQuery = searchQuery,
            IsCompleted = isCompleted,
            IsArchived = isArchived,
            Status = status.HasValue ? (Models.TodoStatus?)status.Value : null,
            IsOverdue = isOverdue,
            HideCompleted = hideCompleted,
            Priority = priority,
            CategoryIds = categoryIds,
            TagIds = tagIds,
            DueDateFrom = dueDateFrom,
            DueDateTo = dueDateTo,
            CreatedAtFrom = createdAtFrom,
            CreatedAtTo = createdAtTo,
            SortBy = sortBy,
            SortOrder = sortOrder,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _todoService.SearchAndFilterTodosAsync(userId.Value, request);
        return Ok(result);
    }

    [HttpPost("reorder")]
    public async Task<ActionResult> ReorderTodos([FromBody] ReorderTodosRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (request.TodoOrders == null || !request.TodoOrders.Any())
            return BadRequest(new { message = "TodoOrders cannot be empty" });

        var success = await _todoService.ReorderTodosAsync(userId.Value, request);
        if (!success)
            return BadRequest(new { message = "Failed to reorder todos. Ensure all todos belong to the user." });

        return Ok(new { message = "Todos reordered successfully" });
    }

    [HttpPost("bulk-complete")]
    public async Task<ActionResult> BulkMarkComplete([FromBody] BulkTodoRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (request.TodoIds == null || !request.TodoIds.Any())
            return BadRequest(new { message = "TodoIds are required" });

        var count = await _todoService.BulkMarkCompleteAsync(userId.Value, request);
        return Ok(new { message = $"{count} todos updated successfully", count });
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<TodoStatisticsDto>> GetStatistics()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var statistics = await _todoService.GetTodoStatisticsAsync(userId.Value);
        return Ok(statistics);
    }

    [HttpPost("archive-old")]
    public async Task<ActionResult> ArchiveOldCompletedTodos([FromQuery] int daysOld = 30)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var count = await _todoService.ArchiveOldCompletedTodosAsync(userId.Value, daysOld);
        return Ok(new { message = $"{count} todos archived successfully", count });
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}



