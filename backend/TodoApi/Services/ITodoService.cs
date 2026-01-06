using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public interface ITodoService
{
    Task<IEnumerable<TodoDto>> GetTodosByUserIdAsync(int userId);
    Task<TodoDto?> GetTodoByIdAsync(int todoId, int userId);
    Task<TodoDto> CreateTodoAsync(CreateTodoRequest request, int userId);
    Task<TodoDto?> UpdateTodoAsync(int todoId, UpdateTodoRequest request, int userId);
    Task<bool> DeleteTodoAsync(int todoId, int userId);
}



