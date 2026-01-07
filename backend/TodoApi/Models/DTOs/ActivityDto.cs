using TodoApi.Models;

namespace TodoApi.Models.DTOs;

public class ActivityDto
{
    public int Id { get; set; }
    public int TodoId { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public ActivityType ActivityType { get; set; }
    public string? Description { get; set; }
    public int? RelatedUserId { get; set; }
    public string? RelatedUserEmail { get; set; }
    public string? RelatedUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

