using System.ComponentModel.DataAnnotations;
using ToucansApi.Core.Models;

namespace ToucansApi.Core.DTOs;

public class TodoListCreateDto
{
    [Required] [StringLength(100)] public string? Name { get; set; }
    public Guid OwnerId { get; set; }
    public string? Description { get; set; }
}

public class TodoListUpdateDto
{
    [Required] [StringLength(100)] public required string Name { get; set; }

    public string? Description { get; set; }
}

public class TodoListResponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public int ItemCount { get; set; } = int.LeadingZeroCount(0);
    public int CompletedItemCount { get; set; }
    public UserResponseDto? Owner { get; set; }
    public SharePermission Permission { get; set; }
    public IEnumerable<TodoItemResponseDto>? Items { get; set; }
}

public class TodoListSummaryDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int ItemCount { get; set; }
    public int CompletedItemCount { get; set; }
}