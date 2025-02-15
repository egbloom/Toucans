using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ToucansApi.Functions.Middleware;

public class HealthCheckMiddleware(HealthCheckService healthCheckService) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpReqData = await context.GetHttpRequestDataAsync();
        if (httpReqData != null && httpReqData.Url.PathAndQuery.EndsWith("/api/health"))
        {
            var health = await healthCheckService.CheckHealthAsync();
            var response = httpReqData.CreateResponse();
            await response.WriteAsJsonAsync(new
            {
                Status = health.Status.ToString(),
                Components = health.Entries
            });
            context.GetInvocationResult().Value = response;
            return;
        }

        await next(context);
    }
}