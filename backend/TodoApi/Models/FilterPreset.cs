namespace TodoApi.Models;

public class FilterPreset
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SearchQuery { get; set; }
    public bool? IsCompleted { get; set; }
    public bool? IsOverdue { get; set; }
    public int? Priority { get; set; }
    public string? CategoryIds { get; set; } // JSON array of category IDs
    public string? TagIds { get; set; } // JSON array of tag IDs
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public User? User { get; set; }
}

