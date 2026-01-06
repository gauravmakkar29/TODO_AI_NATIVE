namespace TodoApi.Models.DTOs;

public class FilterPresetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SearchQuery { get; set; }
    public bool? IsCompleted { get; set; }
    public bool? IsOverdue { get; set; }
    public int? Priority { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<int>? TagIds { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateFilterPresetRequest
{
    public string Name { get; set; } = string.Empty;
    public string? SearchQuery { get; set; }
    public bool? IsCompleted { get; set; }
    public bool? IsOverdue { get; set; }
    public int? Priority { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<int>? TagIds { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}

public class UpdateFilterPresetRequest
{
    public string? Name { get; set; }
    public string? SearchQuery { get; set; }
    public bool? IsCompleted { get; set; }
    public bool? IsOverdue { get; set; }
    public int? Priority { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<int>? TagIds { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}

