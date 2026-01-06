using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
    public async Task<ActionResult<IEnumerable<TodoDto>>> GetTodos()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var todos = await _todoService.GetTodosByUserIdAsync(userId.Value);
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
        [FromQuery] bool? isOverdue,
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
            IsOverdue = isOverdue,
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

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}



