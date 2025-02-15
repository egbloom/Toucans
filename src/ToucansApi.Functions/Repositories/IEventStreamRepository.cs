using ToucansApi.Core.DTOs;

namespace ToucansApi.Functions.Repositories;

public interface IEventStreamRepository
{
    Task<IEnumerable<EventDto>> GetSystemEventsStreamAsync();
    Task<IEnumerable<EventDto>> GetEntityEventsAsync(string entityType, Guid entityId);
    Task<EventDto> PublishEventAsync(EventPublishDto eventDto);
}