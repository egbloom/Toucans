using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ToucansApi.Core.DTOs;
using ToucansApi.Functions.Extensions;
using ToucansApi.Functions.Repositories;

namespace ToucansApi.Functions.Functions;

public class EventStreamFunctions(
    IEventStreamRepository eventRepository,
    ILogger<EventStreamFunctions> logger)
{
    private readonly IEventStreamRepository _eventRepository =
        eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));

    private readonly ILogger<EventStreamFunctions> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Function(nameof(StreamSystemEvents))]
    public async Task<HttpResponseData> StreamSystemEvents(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "events/stream")]
        HttpRequestData req)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(StreamSystemEvents),
            ["Timestamp"] = DateTime.UtcNow
        });

        _logger.LogInformation("Initializing system event stream");

        try
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/event-stream");
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Connection", "keep-alive");

            var events = await _eventRepository.GetSystemEventsStreamAsync();
            await response.WriteAsJsonAsync(events.ToApiResponse());

            _logger.LogInformation("System event stream initialized successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize system event stream. Error: {ErrorMessage}", ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error initializing event stream");
        }
    }

    [Function(nameof(GetEntityEvents))]
    public async Task<HttpResponseData> GetEntityEvents(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "events/{entityType}/{entityId:guid}")]
        HttpRequestData req,
        string entityType,
        Guid entityId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(GetEntityEvents),
            ["EntityType"] = entityType,
            ["EntityId"] = entityId
        });

        _logger.LogInformation("Retrieving events for {EntityType} {EntityId}", entityType, entityId);

        try
        {
            var events = await _eventRepository.GetEntityEventsAsync(entityType, entityId);

            _logger.LogInformation("Retrieved {EventCount} events for {EntityType} {EntityId}",
                events?.Count() ?? 0, entityType, entityId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(events.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to retrieve events for {EntityType} {EntityId}. Error: {ErrorMessage}",
                entityType, entityId, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error retrieving entity events");
        }
    }

    [Function(nameof(PublishEvent))]
    public async Task<HttpResponseData> PublishEvent(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "events")]
        HttpRequestData req)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(PublishEvent),
            ["Timestamp"] = DateTime.UtcNow
        });

        _logger.LogInformation("Publishing new system event");

        try
        {
            var eventDto = await req.ReadFromJsonAsync<EventPublishDto>();
            if (eventDto is null)
            {
                _logger.LogWarning("Invalid event publish request");
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid event data");
            }

            var publishedEvent = await _eventRepository.PublishEventAsync(eventDto);

            _logger.LogInformation("Event published successfully. EventId: {EventId}", publishedEvent.Id);

            var response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Location", $"/api/events/{publishedEvent.Id}");
            await response.WriteAsJsonAsync(publishedEvent.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event. Error: {ErrorMessage}", ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error publishing event");
        }
    }
}