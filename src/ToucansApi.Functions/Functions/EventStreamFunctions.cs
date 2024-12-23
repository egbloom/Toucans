using System.Net;
using Marten;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ToucansApi.Functions.Extensions;

namespace ToucansApi.Functions.Functions;

public class EventStreamFunctions
{
    private readonly ILogger<EventStreamFunctions> _logger;
    private readonly IDocumentStore _store;

    public EventStreamFunctions(IDocumentStore store, ILogger<EventStreamFunctions> logger)
    {
        _store = store;
        _logger = logger;
    }

    [Function("GetEventStream")]
    public async Task<HttpResponseData> GetEventStream(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "events/{streamId}")]
        HttpRequestData req,
        Guid streamId)
    {
        _logger.LogInformation("Fetching event stream for streamId: {StreamId}", streamId);

        await using var session = _store.QuerySession();
        var events = await session.Events.FetchStreamAsync(streamId);

        _logger.LogInformation("Retrieved {EventCount} events for streamId: {StreamId}",
            events.Count, streamId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(events.ToApiResponse());
        return response;
    }

    [Function("GetEventMetrics")]
    public async Task<HttpResponseData> GetEventMetrics(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "events/metrics")]
        HttpRequestData req)
    {
        _logger.LogInformation("Calculating event metrics");

        await using var session = _store.QuerySession();
        var metrics = await session.Events.QueryAllRawEvents()
            .GroupBy(e => e.EventType)
            .Select(g => new
            {
                EventType = g.Key,
                Count = g.Count(),
                LastOccurred = g.Max(e => e.Timestamp)
            })
            .ToListAsync();

        _logger.LogInformation("Generated metrics for {EventTypeCount} event types",
            metrics.Count);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(metrics.ToApiResponse());
        return response;
    }
}