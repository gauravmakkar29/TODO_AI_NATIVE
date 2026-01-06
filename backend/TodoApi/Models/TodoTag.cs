namespace TodoApi.Models;

public class TodoTag
{
    public int TodoId { get; set; }
    public int TagId { get; set; }

    // Navigation properties
    public Todo Todo { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}

