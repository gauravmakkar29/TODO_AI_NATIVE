namespace TodoApi.Models.DTOs;

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTagRequest
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public string? Description { get; set; }
}

public class UpdateTagRequest
{
    public string? Name { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
}

