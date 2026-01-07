namespace TodoApi.Models;

public enum SharePermission
{
    ViewOnly = 0,
    Edit = 1,
    Admin = 2
}

public class TodoShare
{
    public int Id { get; set; }
    public int TodoId { get; set; }
    public int SharedWithUserId { get; set; }
    public int SharedByUserId { get; set; }
    public SharePermission Permission { get; set; } = SharePermission.ViewOnly;
    public bool IsAssigned { get; set; } = false; // True if this is an assignment
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Todo? Todo { get; set; }
    public User? SharedWithUser { get; set; }
    public User? SharedByUser { get; set; }
}

