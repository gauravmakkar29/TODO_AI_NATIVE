using TodoApi.Models;

namespace TodoApi.Models.DTOs;

public class SearchFilterRequest
{
    public string? SearchQuery { get; set; }
    public bool? IsCompleted { get; set; }
    public bool? IsArchived { get; set; }
    public TodoStatus? Status { get; set; }
    public bool? IsOverdue { get; set; } // Special flag for overdue todos
    public bool? HideCompleted { get; set; } // Hide completed tasks from main view
    public int? Priority { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<int>? TagIds { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
    public string? SortBy { get; set; } // "createdAt", "dueDate", "priority", "title"
    public string? SortOrder { get; set; } // "asc", "desc"
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
}

public class SearchFilterResponse
{
    public List<TodoDto> Todos { get; set; } = new List<TodoDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

