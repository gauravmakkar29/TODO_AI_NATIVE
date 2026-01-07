namespace TodoApi.Models.DTOs;

public class BulkTodoRequest
{
    public List<int> TodoIds { get; set; } = new List<int>();
    public bool IsCompleted { get; set; }
}

public class TodoStatisticsDto
{
    public int TotalTodos { get; set; }
    public int CompletedTodos { get; set; }
    public int PendingTodos { get; set; }
    public int ArchivedTodos { get; set; }
    public double CompletionRate { get; set; }
    public int OverdueTodos { get; set; }
    public int HighPriorityTodos { get; set; }
    public int MediumPriorityTodos { get; set; }
    public int LowPriorityTodos { get; set; }
    public Dictionary<string, int> CompletionByDate { get; set; } = new Dictionary<string, int>();
}

