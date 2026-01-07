namespace TodoApi.Models;

public enum TodoStatus
{
    Pending = 0,
    Completed = 1,
    Archived = 2
}

public class Todo
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public TodoStatus Status { get; set; } = TodoStatus.Pending;
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public int Priority { get; set; } = 0; // 0 = Low, 1 = Medium, 2 = High
    public int DisplayOrder { get; set; } = 0; // For drag-and-drop reordering

    // Navigation properties
    public User? User { get; set; }
    public ICollection<TodoCategory> TodoCategories { get; set; } = new List<TodoCategory>();
    public ICollection<TodoTag> TodoTags { get; set; } = new List<TodoTag>();
}



