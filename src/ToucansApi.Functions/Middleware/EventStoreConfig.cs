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
        // Configure Marten
        services.AddMarten(opts =>
            {
                // Connection string from configuration
                opts.Connection(configuration.GetConnectionString("EventStore"));

                // Auto-create schemas in development
                opts.AutoCreateSchemaObjects = AutoCreate.All;

                // Set up database schemas
                opts.DatabaseSchemaName = "public";
                opts.Events.DatabaseSchemaName = "events";

                // Register domain events
                opts.Events.AddEventType<TodoListCreated>();
                opts.Events.AddEventType<TodoItemAdded>();

                // Enable optimistic concurrency
                // opts.Concurrency = ConcurrencyStyle.Optimistic;
            })
            // Integrate with Wolverine
            .IntegrateWithWolverine();

        // Configure Wolverine message bus
        services.AddWolverine(opts =>
        {
            // Set up local queue for event processing
            opts.LocalQueue("events")
                .Sequential()
                .UseDurableInbox();

            // Route all messages to local queue
            opts.PublishAllMessages().ToLocalQueue("events");

            // Auto-discover handlers
            opts.Discovery.IncludeAssembly(typeof(EventStoreConfig).Assembly);
        });

        return services;
    }
}