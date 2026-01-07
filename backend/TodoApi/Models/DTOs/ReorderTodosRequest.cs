namespace TodoApi.Models.DTOs;

public class ReorderTodosRequest
{
    public List<TodoOrderItem> TodoOrders { get; set; } = new List<TodoOrderItem>();
}

public class TodoOrderItem
{
    public int TodoId { get; set; }
    public int DisplayOrder { get; set; }
}


