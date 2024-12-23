using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToucansApi.Core.Models;
using Weasel.Core;
using Wolverine;
using Wolverine.Marten;

namespace ToucansApi.Functions.Middleware;

public static class EventStoreConfig
{
    public static IServiceCollection AddEventStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMarten(opts =>
            {
                opts.Connection(configuration.GetConnectionString("EventStore"));

                opts.AutoCreateSchemaObjects = AutoCreate.All;

                opts.DatabaseSchemaName = "public";
                opts.Events.DatabaseSchemaName = "events";

                opts.Events.AddEventType<TodoListCreated>();
                opts.Events.AddEventType<TodoItemAdded>();

                // opts.Concurrency = ConcurrencyStyle.Optimistic;
            })
            .IntegrateWithWolverine();

        services.AddWolverine(opts =>
        {
            opts.LocalQueue("events")
                .Sequential()
                .UseDurableInbox();

            opts.PublishAllMessages().ToLocalQueue("events");

            opts.Discovery.IncludeAssembly(typeof(EventStoreConfig).Assembly);
        });

        return services;
    }
}