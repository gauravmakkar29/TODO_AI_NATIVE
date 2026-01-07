using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Hubs;
using TodoApi.Models;
using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public class SharingService : ISharingService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IHubContext<TodoHub> _hubContext;

    public SharingService(ApplicationDbContext context, INotificationService notificationService, IHubContext<TodoHub> hubContext)
    {
        _context = context;
        _notificationService = notificationService;
        _hubContext = hubContext;
    }

    public async Task<ShareTodoResponse> ShareTodoAsync(ShareTodoRequest request, int sharedByUserId)
    {
        // Verify the todo exists and belongs to the user sharing it
        var todo = await _context.Todos
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == request.TodoId && t.UserId == sharedByUserId);

        if (todo == null)
            throw new InvalidOperationException("Todo not found or you don't have permission to share it");

        // Verify the user to share with exists
        var sharedWithUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.SharedWithUserId);

        if (sharedWithUser == null)
            throw new InvalidOperationException("User to share with not found");

        // Prevent sharing with yourself
        if (request.SharedWithUserId == sharedByUserId)
            throw new InvalidOperationException("Cannot share a todo with yourself");

        // Check if already shared
        var existingShare = await _context.TodoShares
            .FirstOrDefaultAsync(s => s.TodoId == request.TodoId && s.SharedWithUserId == request.SharedWithUserId);

        if (existingShare != null)
            throw new InvalidOperationException("Todo is already shared with this user");

        // Create the share
        var share = new TodoShare
        {
            TodoId = request.TodoId,
            SharedWithUserId = request.SharedWithUserId,
            SharedByUserId = sharedByUserId,
            Permission = request.Permission,
            IsAssigned = request.IsAssigned,
            CreatedAt = DateTime.UtcNow
        };

        _context.TodoShares.Add(share);
        await _context.SaveChangesAsync();

        // Get shared by user for response and notifications
        var sharedByUser = todo.User ?? await _context.Users.FindAsync(sharedByUserId);

        // Log activity
        var activityDescription = request.IsAssigned 
            ? $"Assigned to {sharedWithUser.Email}" 
            : $"Shared with {sharedWithUser.Email} ({request.Permission} permission)";
        
        await LogActivityAsync(
            request.TodoId, 
            sharedByUserId, 
            request.IsAssigned ? ActivityType.Assigned : ActivityType.Shared,
            activityDescription,
            request.SharedWithUserId);

        // Send SignalR notification to the shared user
        _ = Task.Run(async () =>
        {
            try
            {
                await _hubContext.Clients.Group($"user_{request.SharedWithUserId}")
                    .SendAsync("TodoShared", new { todoId = request.TodoId, sharedBy = sharedByUser?.Email });
                
                await _hubContext.Clients.Group($"todo_{request.TodoId}")
                    .SendAsync("TodoUpdated", new { todoId = request.TodoId, action = "shared" });
            }
            catch (Exception ex)
            {
                // Log error but don't fail the share operation
                Console.WriteLine($"Error sending SignalR notification: {ex.Message}");
            }
        });

        // Return response
        return new ShareTodoResponse
        {
            Id = share.Id,
            TodoId = share.TodoId,
            SharedWithUserId = share.SharedWithUserId,
            SharedWithUserEmail = sharedWithUser.Email,
            SharedWithUserName = $"{sharedWithUser.FirstName} {sharedWithUser.LastName}".Trim(),
            SharedByUserId = share.SharedByUserId,
            SharedByUserEmail = sharedByUser?.Email ?? "",
            Permission = share.Permission,
            IsAssigned = share.IsAssigned,
            CreatedAt = share.CreatedAt
        };
    }

    public async Task<bool> UnshareTodoAsync(int todoId, int sharedWithUserId, int userId)
    {
        // Check if user has permission (must be owner or admin on the share)
        var todo = await _context.Todos.FindAsync(todoId);
        if (todo == null)
            return false;

        var share = await _context.TodoShares
            .FirstOrDefaultAsync(s => s.TodoId == todoId && s.SharedWithUserId == sharedWithUserId);

        if (share == null)
            return false;

        // Only owner or the shared user can unshare
        if (todo.UserId != userId && share.SharedWithUserId != userId)
        {
            // Check if user is admin on this share
            var userShare = await _context.TodoShares
                .FirstOrDefaultAsync(s => s.TodoId == todoId && s.SharedWithUserId == userId && s.Permission == SharePermission.Admin);
            
            if (userShare == null)
                return false;
        }

        var relatedUserId = share.SharedWithUserId;
        var isAssigned = share.IsAssigned;

        _context.TodoShares.Remove(share);
        await _context.SaveChangesAsync();

        // Log activity
        await LogActivityAsync(
            todoId,
            userId,
            isAssigned ? ActivityType.Unassigned : ActivityType.Unshared,
            isAssigned ? "Unassigned" : "Unshared",
            relatedUserId);

        // Send SignalR notification
        _ = Task.Run(async () =>
        {
            try
            {
                await _hubContext.Clients.Group($"user_{relatedUserId}")
                    .SendAsync("TodoUnshared", new { todoId = todoId });
                
                await _hubContext.Clients.Group($"todo_{todoId}")
                    .SendAsync("TodoUpdated", new { todoId = todoId, action = "unshared" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SignalR notification: {ex.Message}");
            }
        });

        return true;
    }

    public async Task<bool> UpdateSharePermissionAsync(int todoId, int sharedWithUserId, UpdateSharePermissionRequest request, int userId)
    {
        var todo = await _context.Todos.FindAsync(todoId);
        if (todo == null)
            return false;

        // Only owner can update permissions
        if (todo.UserId != userId)
        {
            // Or check if user is admin on this share
            var userShare = await _context.TodoShares
                .FirstOrDefaultAsync(s => s.TodoId == todoId && s.SharedWithUserId == userId && s.Permission == SharePermission.Admin);
            
            if (userShare == null)
                return false;
        }

        var share = await _context.TodoShares
            .FirstOrDefaultAsync(s => s.TodoId == todoId && s.SharedWithUserId == sharedWithUserId);

        if (share == null)
            return false;

        var oldPermission = share.Permission;
        share.Permission = request.Permission;
        share.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log activity
        await LogActivityAsync(
            todoId,
            userId,
            ActivityType.PermissionChanged,
            $"Permission changed from {oldPermission} to {request.Permission}",
            sharedWithUserId);

        return true;
    }

    public async Task<IEnumerable<ShareTodoResponse>> GetTodoSharesAsync(int todoId, int userId)
    {
        // Verify user has access to this todo
        var todo = await _context.Todos.FindAsync(todoId);
        if (todo == null)
            return Enumerable.Empty<ShareTodoResponse>();

        // Only owner can see all shares
        if (todo.UserId != userId)
        {
            // Check if user has access via share
            var userShare = await _context.TodoShares
                .FirstOrDefaultAsync(s => s.TodoId == todoId && s.SharedWithUserId == userId);
            
            if (userShare == null || userShare.Permission != SharePermission.Admin)
                return Enumerable.Empty<ShareTodoResponse>();
        }

        var shares = await _context.TodoShares
            .Include(s => s.SharedWithUser)
            .Include(s => s.SharedByUser)
            .Where(s => s.TodoId == todoId)
            .ToListAsync();

        return shares.Select(s => new ShareTodoResponse
        {
            Id = s.Id,
            TodoId = s.TodoId,
            SharedWithUserId = s.SharedWithUserId,
            SharedWithUserEmail = s.SharedWithUser?.Email ?? "",
            SharedWithUserName = $"{s.SharedWithUser?.FirstName} {s.SharedWithUser?.LastName}".Trim(),
            SharedByUserId = s.SharedByUserId,
            SharedByUserEmail = s.SharedByUser?.Email ?? "",
            Permission = s.Permission,
            IsAssigned = s.IsAssigned,
            CreatedAt = s.CreatedAt
        });
    }

    public async Task<IEnumerable<SharedTodoDto>> GetSharedTodosAsync(int userId)
    {
        var sharedTodos = await _context.TodoShares
            .Include(s => s.Todo)
                .ThenInclude(t => t!.User)
            .Include(s => s.Todo!)
                .ThenInclude(t => t.TodoCategories)
                    .ThenInclude(tc => tc.Category)
            .Include(s => s.Todo!)
                .ThenInclude(t => t.TodoTags)
                    .ThenInclude(tt => tt.Tag)
            .Include(s => s.Todo!)
                .ThenInclude(t => t.TodoShares)
                    .ThenInclude(ts => ts.SharedWithUser)
            .Where(s => s.SharedWithUserId == userId)
            .Select(s => s.Todo!)
            .Distinct()
            .ToListAsync();

        var result = new List<SharedTodoDto>();

        foreach (var todo in sharedTodos)
        {
            var userShare = await _context.TodoShares
                .FirstOrDefaultAsync(s => s.TodoId == todo.Id && s.SharedWithUserId == userId);

            var shares = await _context.TodoShares
                .Include(s => s.SharedWithUser)
                .Include(s => s.SharedByUser)
                .Where(s => s.TodoId == todo.Id)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var isOverdue = todo.DueDate.HasValue && !todo.IsCompleted && todo.DueDate.Value < now;
            var isApproachingDue = todo.DueDate.HasValue && !todo.IsCompleted && !isOverdue &&
                                 todo.DueDate.Value <= now.AddDays(3) && todo.DueDate.Value >= now;

            result.Add(new SharedTodoDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                Status = todo.Status,
                IsArchived = todo.IsArchived,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt,
                CompletedAt = todo.CompletedAt,
                ArchivedAt = todo.ArchivedAt,
                DueDate = todo.DueDate,
                ReminderDate = todo.ReminderDate,
                Priority = todo.Priority,
                IsOverdue = isOverdue,
                IsApproachingDue = isApproachingDue,
                OwnerUserId = todo.UserId,
                OwnerEmail = todo.User?.Email ?? "",
                OwnerName = $"{todo.User?.FirstName} {todo.User?.LastName}".Trim(),
                UserPermission = userShare?.Permission,
                IsAssignedToUser = userShare?.IsAssigned ?? false,
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
                    .ToList(),
                SharedWith = shares.Select(s => new TodoShareInfoDto
                {
                    Id = s.Id,
                    SharedWithUserId = s.SharedWithUserId,
                    SharedWithUserEmail = s.SharedWithUser?.Email ?? "",
                    SharedWithUserName = $"{s.SharedWithUser?.FirstName} {s.SharedWithUser?.LastName}".Trim(),
                    SharedByUserId = s.SharedByUserId,
                    SharedByUserEmail = s.SharedByUser?.Email ?? "",
                    Permission = s.Permission,
                    IsAssigned = s.IsAssigned,
                    CreatedAt = s.CreatedAt
                }).ToList()
            });
        }

        return result;
    }

    public async Task<bool> CanUserAccessTodoAsync(int todoId, int userId)
    {
        // Check if user owns the todo
        var todo = await _context.Todos.FindAsync(todoId);
        if (todo == null)
            return false;

        if (todo.UserId == userId)
            return true;

        // Check if todo is shared with user
        var share = await _context.TodoShares
            .AnyAsync(s => s.TodoId == todoId && s.SharedWithUserId == userId);

        return share;
    }

    public async Task<SharePermission?> GetUserPermissionAsync(int todoId, int userId)
    {
        var todo = await _context.Todos.FindAsync(todoId);
        if (todo == null)
            return null;

        // Owner has admin permission
        if (todo.UserId == userId)
            return SharePermission.Admin;

        var share = await _context.TodoShares
            .FirstOrDefaultAsync(s => s.TodoId == todoId && s.SharedWithUserId == userId);

        return share?.Permission;
    }

    public async Task LogActivityAsync(int todoId, int userId, ActivityType activityType, string? description = null, int? relatedUserId = null)
    {
        var activity = new TodoActivity
        {
            TodoId = todoId,
            UserId = userId,
            ActivityType = activityType,
            Description = description,
            RelatedUserId = relatedUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.TodoActivities.Add(activity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ActivityDto>> GetTodoActivitiesAsync(int todoId, int userId)
    {
        // Verify user has access
        if (!await CanUserAccessTodoAsync(todoId, userId))
            return Enumerable.Empty<ActivityDto>();

        var activities = await _context.TodoActivities
            .Include(a => a.User)
            .Include(a => a.RelatedUser)
            .Where(a => a.TodoId == todoId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return activities.Select(a => new ActivityDto
        {
            Id = a.Id,
            TodoId = a.TodoId,
            UserId = a.UserId,
            UserEmail = a.User?.Email ?? "",
            UserName = $"{a.User?.FirstName} {a.User?.LastName}".Trim(),
            ActivityType = a.ActivityType,
            Description = a.Description,
            RelatedUserId = a.RelatedUserId,
            RelatedUserEmail = a.RelatedUser?.Email ?? "",
            RelatedUserName = $"{a.RelatedUser?.FirstName} {a.RelatedUser?.LastName}".Trim(),
            CreatedAt = a.CreatedAt
        });
    }
}

