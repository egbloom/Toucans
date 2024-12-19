using ToucansApi.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace ToucansApi.Core.Models
    
{
public class User
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string? FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; }
        
    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<TodoList>? OwnedLists { get; set; }
    public virtual ICollection<TodoListShare>? SharedLists { get; set; }
}
}