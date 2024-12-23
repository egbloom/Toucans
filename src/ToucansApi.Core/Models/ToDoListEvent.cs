namespace ToucansApi.Core.Models;

// New event tracking class
public class TodoListEvent
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid TodoListId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public Guid UserId { get; set; }

    public virtual TodoList TodoList { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}