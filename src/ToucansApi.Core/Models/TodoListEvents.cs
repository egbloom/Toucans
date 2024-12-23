namespace ToucansApi.Core.Models;

public record TodoListCreated
{
    public Guid ListId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid OwnerId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record TodoItemAdded
{
    public Guid ItemId { get; init; }
    public Guid ListId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? AssignedToId { get; init; }
}