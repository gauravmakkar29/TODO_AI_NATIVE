namespace TodoApi.Models.DTOs;

public class CreateCommentRequest
{
    public int TodoId { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class UpdateCommentRequest
{
    public string Comment { get; set; } = string.Empty;
}

public class CommentDto
{
    public int Id { get; set; }
    public int TodoId { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

