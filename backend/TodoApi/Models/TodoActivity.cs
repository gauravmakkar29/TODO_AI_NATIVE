namespace TodoApi.Models;

public enum ActivityType
{
    Created = 0,
    Updated = 1,
    Completed = 2,
    Uncompleted = 3,
    Deleted = 4,
    Shared = 5,
    Unshared = 6,
    Assigned = 7,
    Unassigned = 8,
    CommentAdded = 9,
    PermissionChanged = 10
}

public class TodoActivity
{
    public int Id { get; set; }
    public int TodoId { get; set; }
    public int UserId { get; set; } // User who performed the action
    public ActivityType ActivityType { get; set; }
    public string? Description { get; set; } // Additional details about the activity
    public int? RelatedUserId { get; set; } // For share/assign activities
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Todo? Todo { get; set; }
    public User? User { get; set; }
    public User? RelatedUser { get; set; }
}

