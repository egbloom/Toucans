using System.ComponentModel.DataAnnotations;

namespace ToucansApi.Core.Models;

public class TodoList
{
    public Guid Id { get; set; } = Guid.Empty;

    [Required] [StringLength(100)] public required string Name { get; set; }

    [StringLength(256)] public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public Guid OwnerId { get; set; }

    public virtual User? Owner { get; set; }
    public virtual ICollection<TodoItem>? Items { get; set; }
    public ICollection<TodoListShare> Shares { get; set; } = new List<TodoListShare>();
}