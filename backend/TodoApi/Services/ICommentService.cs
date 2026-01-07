using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public interface ICommentService
{
    Task<CommentDto> CreateCommentAsync(CreateCommentRequest request, int userId);
    Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentRequest request, int userId);
    Task<bool> DeleteCommentAsync(int commentId, int userId);
    Task<IEnumerable<CommentDto>> GetTodoCommentsAsync(int todoId, int userId);
    Task<CommentDto?> GetCommentByIdAsync(int commentId, int userId);
}

