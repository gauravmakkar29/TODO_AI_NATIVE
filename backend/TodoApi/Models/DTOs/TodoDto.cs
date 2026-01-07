using TodoApi.Models;

namespace TodoApi.Models.DTOs;

public class TodoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public TodoStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public int Priority { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsApproachingDue { get; set; }
    public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    public List<TagDto> Tags { get; set; } = new List<TagDto>();
}

public class CreateTodoRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public int Priority { get; set; } = 0;
    public List<int> CategoryIds { get; set; } = new List<int>();
    public List<int> TagIds { get; set; } = new List<int>();
}

public class UpdateTodoRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public int? Priority { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<int>? TagIds { get; set; }
}



