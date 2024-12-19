using ToucansApi.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace ToucansApi.Core.Models

{
public class TodoItem
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public required string Title { get; set; }

    public required string Description { get; set; }

    public DateTime CreatedAt { get; set; }
        
    public DateTime? DueDate { get; set; }
        
    public DateTime? CompletedAt { get; set; }

    public Priority Priority { get; set; }

    public TodoStatus Status { get; set; }

    public Guid ListId { get; set; }
    public Guid? AssignedToId { get; set; }
    
    public virtual TodoList? List { get; set; }
    public virtual User? AssignedTo { get; set; }
}
}