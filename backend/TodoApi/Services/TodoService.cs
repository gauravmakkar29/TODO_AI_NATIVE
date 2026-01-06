using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public class TodoService : ITodoService
{
    private readonly ApplicationDbContext _context;

    public TodoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TodoDto>> GetTodosByUserIdAsync(int userId)
    {
        var todos = await _context.Todos
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TodoDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                DueDate = t.DueDate,
                Priority = t.Priority
            })
            .ToListAsync();

        return todos;
    }

    public async Task<TodoDto?> GetTodoByIdAsync(int todoId, int userId)
    {
        var todo = await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId);

        if (todo == null)
            return null;

        return new TodoDto
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt,
            UpdatedAt = todo.UpdatedAt,
            DueDate = todo.DueDate,
            Priority = todo.Priority
        };
    }

    public async Task<TodoDto> CreateTodoAsync(CreateTodoRequest request, int userId)
    {
        var todo = new Todo
        {
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = request.Priority,
            CreatedAt = DateTime.UtcNow
        };

        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        return new TodoDto
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt,
            UpdatedAt = todo.UpdatedAt,
            DueDate = todo.DueDate,
            Priority = todo.Priority
        };
    }

    public async Task<TodoDto?> UpdateTodoAsync(int todoId, UpdateTodoRequest request, int userId)
    {
        var todo = await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId);

        if (todo == null)
            return null;

        if (!string.IsNullOrWhiteSpace(request.Title))
            todo.Title = request.Title;

        if (request.Description != null)
            todo.Description = request.Description;

        if (request.IsCompleted.HasValue)
            todo.IsCompleted = request.IsCompleted.Value;

        if (request.DueDate.HasValue)
            todo.DueDate = request.DueDate;

        if (request.Priority.HasValue)
            todo.Priority = request.Priority.Value;

        todo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new TodoDto
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt,
            UpdatedAt = todo.UpdatedAt,
            DueDate = todo.DueDate,
            Priority = todo.Priority
        };
    }

    public async Task<bool> DeleteTodoAsync(int todoId, int userId)
    {
        var todo = await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId);

        if (todo == null)
            return false;

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();

        return true;
    }
}



