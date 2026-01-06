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

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}



