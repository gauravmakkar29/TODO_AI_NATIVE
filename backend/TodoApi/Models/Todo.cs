namespace TodoApi.Models;

public class Todo
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; } = 0; // 0 = Low, 1 = Medium, 2 = High

    // Navigation property
    public User? User { get; set; }
}



