using TodoApi.Models;

namespace TodoApi.Models.DTOs;

public class ShareTodoRequest
{
    public int TodoId { get; set; }
    public int SharedWithUserId { get; set; }
    public SharePermission Permission { get; set; } = SharePermission.ViewOnly;
    public bool IsAssigned { get; set; } = false;
}

public class ShareTodoResponse
{
    public int Id { get; set; }
    public int TodoId { get; set; }
    public int SharedWithUserId { get; set; }
    public string SharedWithUserEmail { get; set; } = string.Empty;
    public string? SharedWithUserName { get; set; }
    public int SharedByUserId { get; set; }
    public string SharedByUserEmail { get; set; } = string.Empty;
    public SharePermission Permission { get; set; }
    public bool IsAssigned { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateSharePermissionRequest
{
    public SharePermission Permission { get; set; }
}

public class TodoShareInfoDto
{
    public int Id { get; set; }
    public int SharedWithUserId { get; set; }
    public string SharedWithUserEmail { get; set; } = string.Empty;
    public string? SharedWithUserName { get; set; }
    public int SharedByUserId { get; set; }
    public string SharedByUserEmail { get; set; } = string.Empty;
    public SharePermission Permission { get; set; }
    public bool IsAssigned { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SharedTodoDto : TodoDto
{
    public int OwnerUserId { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public SharePermission? UserPermission { get; set; } // Permission for the current user
    public bool IsAssignedToUser { get; set; }
    public List<TodoShareInfoDto> SharedWith { get; set; } = new List<TodoShareInfoDto>();
}

