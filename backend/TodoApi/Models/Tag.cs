namespace TodoApi.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000"; // Hex color code
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property for many-to-many relationship
    public ICollection<TodoTag> TodoTags { get; set; } = new List<TodoTag>();
}

