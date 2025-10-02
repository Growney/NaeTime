using EventDbLite;
using EventDbLite.Abstractions;
using EventDbLite.Aggregates;
using EventDbLite.Connections;
using EventDbLite.Events;
using EventDbLite.Handlers;
using EventDbLite.Projections;
using EventDbLite.Reactions;
using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.SQLite;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{

    public static IServiceCollection AddEventDbLite(this IServiceCollection services)
    {
        services.AddDbContext<EventDbLiteContext>(options =>
        {
            options.UseSqlite("Data Source=eventdblite.db;Cache=Shared;Pooling=true;")
                   .EnableSensitiveDataLogging()
                   .EnableDetailedErrors();
        });
        services.AddHostedService<SQLiteDatabaseManager<EventDbLiteContext>>();

        services.AddSingleton<IEventStoreLite, EventStoreLite>();

        services.AddSingleton<IEventSerializer, JsonEventSerializer>();
        services.AddSingleton<IHandlerProvider, HandlerProvider>();
        services.AddSingleton<IAsyncHandlerProvider, AsyncHandlerProvider>();

        services.AddScoped<IEventStreamConnection, EventStreamConnection>();
        services.AddScoped<IAggregateRepository, AggregateRepository>();
        services.AddScoped<IProjectionProvider, ProjectionProvider>();
        services.AddScoped<IReactionProvider, ReactionProvider>();

        services.AddHostedService<LiveProjectionService>();
        return services;
    }

    private static IServiceCollection AddLiveProjection(this IServiceCollection services, Type projectionType, ServiceLifetime lifetime, string? streamName = null)
    {
        services.Add(new ServiceDescriptor(projectionType,
            provider =>
            {
                LiveProjection projection = (LiveProjection)ActivatorUtilities.CreateInstance(provider, projectionType);
                projection._eventSerializer = provider.GetRequiredService<IEventSerializer>();
                projection._handlerProvider = provider.GetRequiredService<IAsyncHandlerProvider>();
                return projection;
            }, lifetime));

        services.AddSingleton(new LiveProjectionRequirement(streamName, projectionType));
        return services;
    }


    public static IServiceCollection AddSingletonLiveProjection<T>(this IServiceCollection services, string? streamName = null) => AddLiveProjection(services, typeof(T), ServiceLifetime.Singleton, streamName);
    public static IServiceCollection AddScopedLiveProjection<T>(this IServiceCollection services, string? streamName = null) => AddLiveProjection(services, typeof(T), ServiceLifetime.Scoped, streamName);
    public static IServiceCollection AddTransientLiveProjection<T>(this IServiceCollection services, string? streamName = null) => AddLiveProjection(services, typeof(T), ServiceLifetime.Transient, streamName);

    public static IServiceCollection AddConstantReaction<T>(this IServiceCollection services, Func<T, Task> reaction)
    {
        services.AddSingleton((serviceProvider) =>
        {
            IReactionProvider reactionProvider = serviceProvider.GetRequiredService<IReactionProvider>();

            return reactionProvider.On(reaction);
        });

        return services;
    }
}
