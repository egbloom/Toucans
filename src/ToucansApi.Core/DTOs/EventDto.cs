namespace ToucansApi.Core.DTOs;

public class EventDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Data { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public class EventPublishDto
{
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Data { get; set; } = string.Empty;
}