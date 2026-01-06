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
            .Include(t => t.TodoCategories)
                .ThenInclude(tc => tc.Category)
            .Include(t => t.TodoTags)
                .ThenInclude(tt => tt.Tag)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return todos.Select(t => MapToDto(t));
    }

    public async Task<TodoDto?> GetTodoByIdAsync(int todoId, int userId)
    {
        var todo = await _context.Todos
            .Include(t => t.TodoCategories)
                .ThenInclude(tc => tc.Category)
            .Include(t => t.TodoTags)
                .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId);

        if (todo == null)
            return null;

        return MapToDto(todo);
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

        // Assign categories
        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            var validCategoryIds = await _context.Categories
                .Where(c => request.CategoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var categoryId in validCategoryIds)
            {
                todo.TodoCategories.Add(new TodoCategory
                {
                    TodoId = todo.Id,
                    CategoryId = categoryId
                });
            }
        }

        // Assign tags
        if (request.TagIds != null && request.TagIds.Any())
        {
            var validTagIds = await _context.Tags
                .Where(t => request.TagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            foreach (var tagId in validTagIds)
            {
                todo.TodoTags.Add(new TodoTag
                {
                    TodoId = todo.Id,
                    TagId = tagId
                });
            }
        }

        await _context.SaveChangesAsync();

        // Reload with relationships
        await _context.Entry(todo)
            .Collection(t => t.TodoCategories)
            .Query()
            .Include(tc => tc.Category)
            .LoadAsync();

        await _context.Entry(todo)
            .Collection(t => t.TodoTags)
            .Query()
            .Include(tt => tt.Tag)
            .LoadAsync();

        return MapToDto(todo);
    }

    public async Task<TodoDto?> UpdateTodoAsync(int todoId, UpdateTodoRequest request, int userId)
    {
        var todo = await _context.Todos
            .Include(t => t.TodoCategories)
            .Include(t => t.TodoTags)
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

        // Update categories if provided
        if (request.CategoryIds != null)
        {
            // Remove existing categories
            _context.TodoCategories.RemoveRange(todo.TodoCategories);

            // Add new categories
            if (request.CategoryIds.Any())
            {
                var validCategoryIds = await _context.Categories
                    .Where(c => request.CategoryIds.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var categoryId in validCategoryIds)
                {
                    todo.TodoCategories.Add(new TodoCategory
                    {
                        TodoId = todo.Id,
                        CategoryId = categoryId
                    });
                }
            }
        }

        // Update tags if provided
        if (request.TagIds != null)
        {
            // Remove existing tags
            _context.TodoTags.RemoveRange(todo.TodoTags);

            // Add new tags
            if (request.TagIds.Any())
            {
                var validTagIds = await _context.Tags
                    .Where(t => request.TagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                foreach (var tagId in validTagIds)
                {
                    todo.TodoTags.Add(new TodoTag
                    {
                        TodoId = todo.Id,
                        TagId = tagId
                    });
                }
            }
        }

        todo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload with relationships
        await _context.Entry(todo)
            .Collection(t => t.TodoCategories)
            .Query()
            .Include(tc => tc.Category)
            .LoadAsync();

        await _context.Entry(todo)
            .Collection(t => t.TodoTags)
            .Query()
            .Include(tt => tt.Tag)
            .LoadAsync();

        return MapToDto(todo);
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

    public async Task<IEnumerable<TodoDto>> GetTodosByCategoryAsync(int userId, int categoryId)
    {
        var todos = await _context.Todos
            .Where(t => t.UserId == userId && t.TodoCategories.Any(tc => tc.CategoryId == categoryId))
            .Include(t => t.TodoCategories)
                .ThenInclude(tc => tc.Category)
            .Include(t => t.TodoTags)
                .ThenInclude(tt => tt.Tag)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return todos.Select(t => MapToDto(t));
    }

    public async Task<IEnumerable<TodoDto>> GetTodosByTagAsync(int userId, int tagId)
    {
        var todos = await _context.Todos
            .Where(t => t.UserId == userId && t.TodoTags.Any(tt => tt.TagId == tagId))
            .Include(t => t.TodoCategories)
                .ThenInclude(tc => tc.Category)
            .Include(t => t.TodoTags)
                .ThenInclude(tt => tt.Tag)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return todos.Select(t => MapToDto(t));
    }

    private TodoDto MapToDto(Todo todo)
    {
        return new TodoDto
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt,
            UpdatedAt = todo.UpdatedAt,
            DueDate = todo.DueDate,
            Priority = todo.Priority,
            Categories = todo.TodoCategories
                .Select(tc => new CategoryDto
                {
                    Id = tc.Category.Id,
                    Name = tc.Category.Name,
                    Color = tc.Category.Color,
                    Description = tc.Category.Description,
                    CreatedAt = tc.Category.CreatedAt,
                    UpdatedAt = tc.Category.UpdatedAt
                })
                .ToList(),
            Tags = todo.TodoTags
                .Select(tt => new TagDto
                {
                    Id = tt.Tag.Id,
                    Name = tt.Tag.Name,
                    Color = tt.Tag.Color,
                    Description = tt.Tag.Description,
                    CreatedAt = tt.Tag.CreatedAt,
                    UpdatedAt = tt.Tag.UpdatedAt
                })
                .ToList()
        };
    }
}



