using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToucansApi.Core.Models;

public class TodoListShare
{
    public Guid Id { get; init; }

    [Required] public Guid TodoListId { get; init; }

    [ForeignKey("TodoListId")] public required TodoList TodoList { get; init; }

    [Required] public Guid SharedWithUserId { get; init; }

    [ForeignKey("SharedWithUserId")] public required User SharedWithUser { get; init; }

    [Required] public SharePermission Permission { get; init; }

    public DateTime CreatedAt { get; init; }
}