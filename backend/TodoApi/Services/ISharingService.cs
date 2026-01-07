using TodoApi.Models;
using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public interface ISharingService
{
    Task<ShareTodoResponse> ShareTodoAsync(ShareTodoRequest request, int sharedByUserId);
    Task<bool> UnshareTodoAsync(int todoId, int sharedWithUserId, int userId);
    Task<bool> UpdateSharePermissionAsync(int todoId, int sharedWithUserId, UpdateSharePermissionRequest request, int userId);
    Task<IEnumerable<ShareTodoResponse>> GetTodoSharesAsync(int todoId, int userId);
    Task<IEnumerable<SharedTodoDto>> GetSharedTodosAsync(int userId);
    Task<bool> CanUserAccessTodoAsync(int todoId, int userId);
    Task<SharePermission?> GetUserPermissionAsync(int todoId, int userId);
    Task LogActivityAsync(int todoId, int userId, ActivityType activityType, string? description = null, int? relatedUserId = null);
    Task<IEnumerable<ActivityDto>> GetTodoActivitiesAsync(int todoId, int userId);
}

