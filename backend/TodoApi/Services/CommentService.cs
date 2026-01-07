using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Hubs;
using TodoApi.Models;
using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _context;
    private readonly ISharingService _sharingService;
    private readonly IHubContext<TodoHub> _hubContext;

    public CommentService(ApplicationDbContext context, ISharingService sharingService, IHubContext<TodoHub> hubContext)
    {
        _context = context;
        _sharingService = sharingService;
        _hubContext = hubContext;
    }

    public async Task<CommentDto> CreateCommentAsync(CreateCommentRequest request, int userId)
    {
        // Verify user has access to the todo
        if (!await _sharingService.CanUserAccessTodoAsync(request.TodoId, userId))
            throw new UnauthorizedAccessException("You don't have access to this todo");

        // Verify user has permission to comment (at least ViewOnly)
        var permission = await _sharingService.GetUserPermissionAsync(request.TodoId, userId);
        if (permission == null)
        {
            // Check if user owns the todo
            var todo = await _context.Todos.FindAsync(request.TodoId);
            if (todo == null || todo.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to comment on this todo");
        }

        var comment = new TodoComment
        {
            TodoId = request.TodoId,
            UserId = userId,
            Comment = request.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.TodoComments.Add(comment);
        await _context.SaveChangesAsync();

        // Log activity
        await _sharingService.LogActivityAsync(
            request.TodoId,
            userId,
            ActivityType.CommentAdded,
            $"Added a comment");

        // Reload with user info
        await _context.Entry(comment)
            .Reference(c => c.User)
            .LoadAsync();

        var commentDto = MapToDto(comment);

        // Send SignalR notification
        _ = Task.Run(async () =>
        {
            try
            {
                await _hubContext.Clients.Group($"todo_{request.TodoId}")
                    .SendAsync("CommentAdded", commentDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SignalR notification: {ex.Message}");
            }
        });

        return commentDto;
    }

    public async Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentRequest request, int userId)
    {
        var comment = await _context.TodoComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

        if (comment == null)
            return null;

        // Verify user still has access to the todo
        if (!await _sharingService.CanUserAccessTodoAsync(comment.TodoId, userId))
            throw new UnauthorizedAccessException("You don't have access to this todo");

        comment.Comment = request.Comment.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(comment);
    }

    public async Task<bool> DeleteCommentAsync(int commentId, int userId)
    {
        var comment = await _context.TodoComments
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            return false;

        // Only the comment owner or todo owner can delete
        var todo = await _context.Todos.FindAsync(comment.TodoId);
        if (comment.UserId != userId && (todo == null || todo.UserId != userId))
            return false;

        // Verify user has access to the todo
        if (!await _sharingService.CanUserAccessTodoAsync(comment.TodoId, userId))
            return false;

        _context.TodoComments.Remove(comment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CommentDto>> GetTodoCommentsAsync(int todoId, int userId)
    {
        // Verify user has access to the todo
        if (!await _sharingService.CanUserAccessTodoAsync(todoId, userId))
            return Enumerable.Empty<CommentDto>();

        var comments = await _context.TodoComments
            .Include(c => c.User)
            .Where(c => c.TodoId == todoId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(c => MapToDto(c));
    }

    public async Task<CommentDto?> GetCommentByIdAsync(int commentId, int userId)
    {
        var comment = await _context.TodoComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            return null;

        // Verify user has access to the todo
        if (!await _sharingService.CanUserAccessTodoAsync(comment.TodoId, userId))
            return null;

        return MapToDto(comment);
    }

    private CommentDto MapToDto(TodoComment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            TodoId = comment.TodoId,
            UserId = comment.UserId,
            UserEmail = comment.User?.Email ?? "",
            UserName = $"{comment.User?.FirstName} {comment.User?.LastName}".Trim(),
            Comment = comment.Comment,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}

