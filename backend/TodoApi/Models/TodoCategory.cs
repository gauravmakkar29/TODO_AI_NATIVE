namespace TodoApi.Models;

public class TodoCategory
{
    public int TodoId { get; set; }
    public int CategoryId { get; set; }

    // Navigation properties
    public Todo Todo { get; set; } = null!;
    public Category Category { get; set; } = null!;
}

