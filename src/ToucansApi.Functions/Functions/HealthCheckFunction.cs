using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace ToucansApi.Functions.Functions;

public class HealthCheckFunction(
    HealthCheckService healthCheckService,
    ILogger<HealthCheckFunction> logger)
{
    [Function("HealthCheck")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "health")]
        HttpRequestData req,
        FunctionContext executionContext)
    {
        logger.LogInformation("Health check requested at: {Timestamp}", DateTime.UtcNow);

        var result = await healthCheckService.CheckHealthAsync();

        logger.LogInformation(
            "Health check completed. Status: {Status}, Components checked: {ComponentCount}",
            result.Status,
            result.Entries.Count);

        if (result.Status != HealthStatus.Healthy)
        {
            var unhealthyComponents = result.Entries
                .Where(e => e.Value.Status != HealthStatus.Healthy)
                .Select(e => e.Key)
                .ToList();

            if (unhealthyComponents.Any())
                logger.LogWarning(
                    "Unhealthy components detected: {UnhealthyComponents}",
                    string.Join(", ", unhealthyComponents));
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            Status = result.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Components = result.Entries.ToDictionary(
                e => e.Key,
                e => new ComponentStatus
                {
                    Status = e.Value.Status.ToString(),
                    Description = e.Value.Description
                }
            )
        });

        return response;
    }

    private class ComponentStatus
    {
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}