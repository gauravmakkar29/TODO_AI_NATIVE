namespace TodoApi.Models;

public class TodoComment
{
    public int Id { get; set; }
    public int TodoId { get; set; }
    public int UserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Todo? Todo { get; set; }
    public User? User { get; set; }
}

