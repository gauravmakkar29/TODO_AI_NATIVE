using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public interface ITodoService
{
    Task<IEnumerable<TodoDto>> GetTodosByUserIdAsync(int userId, string? sortBy = null, int? priorityFilter = null);
    Task<TodoDto?> GetTodoByIdAsync(int todoId, int userId);
    Task<TodoDto> CreateTodoAsync(CreateTodoRequest request, int userId);
    Task<TodoDto?> UpdateTodoAsync(int todoId, UpdateTodoRequest request, int userId);
    Task<bool> DeleteTodoAsync(int todoId, int userId);
    Task<IEnumerable<TodoDto>> GetTodosByCategoryAsync(int userId, int categoryId);
    Task<IEnumerable<TodoDto>> GetTodosByTagAsync(int userId, int tagId);
    Task<SearchFilterResponse> SearchAndFilterTodosAsync(int userId, SearchFilterRequest request);
}



