using EventDbLite;
using EventDbLite.Abstractions;
using EventDbLite.Aggregates;
using EventDbLite.Connections;
using EventDbLite.Events;
using EventDbLite.Handlers;
using EventDbLite.Projections;
using EventDbLite.Reactions;
using EventDbLite.Streams;
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

        services.AddHostedService<ReactionService>();

        services.AddSingleton<IEventStoreLite, EventStoreLite>();

        services.AddSingleton<IEventSerializer, JsonEventSerializer>();
        services.AddSingleton<IHandlerProvider, HandlerProvider>();
        services.AddSingleton<IAsyncHandlerProvider, AsyncHandlerProvider>();

        services.AddTransient<IEventStreamConnection, EventStreamConnection>();
        services.AddTransient<IAggregateRepository, AggregateRepository>();
        services.AddTransient<IProjectionProvider, ProjectionProvider>();

        services.AddTransient<IStreamEventWriter, StreamEventWriter>();
        services.AddTransient<IReactionProviderFactory, ReactionProviderFactory>();
        services.AddTransient(x => x.GetRequiredService<IReactionProviderFactory>().CreateProvider(StreamPosition.End));

        services.AddHostedService<LiveProjectionService>();
        return services;
    }

    private static IServiceCollection AddLiveProjection(this IServiceCollection services, Type projectionType, string? streamName = null)
    {
        services.AddSingleton(new LiveProjectionRequirement(streamName, projectionType));
        return services;
    }
    public static IServiceCollection AddSingletonLiveProjection<TService, TImplementation>(this IServiceCollection services, string? streamName = null)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddSingleton<TService, TImplementation>();
        services.AddLiveProjection(typeof(TService), streamName);

        return services;
    }
    public static IServiceCollection AddSingletonLiveProjection<TImplementation>(this IServiceCollection services, string? streamName = null)
        where TImplementation : class
    {
        services.AddScoped<TImplementation>();
        services.AddLiveProjection(typeof(TImplementation), streamName);
        return services;
    }
    public static IServiceCollection AddScopedLiveProjection<TImplementation>(this IServiceCollection services, string? streamName = null)
        where TImplementation : class
    {
        services.AddScoped<TImplementation>();
        services.AddLiveProjection(typeof(TImplementation), streamName);
        return services;
    }
    public static IServiceCollection AddScopedLiveProjection<TService, TImplementation>(this IServiceCollection services, string? streamName = null)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddScoped<TService, TImplementation>();
        services.AddLiveProjection(typeof(TService), streamName);
        return services;
    }
    public static IServiceCollection AddTransientLiveProjection<TImplementation>(this IServiceCollection services, string? streamName = null)
         where TImplementation : class
    {
        services.AddTransient<TImplementation>();
        services.AddLiveProjection(typeof(TImplementation), streamName);
        return services;
    }
    public static IServiceCollection AddTransientLiveProjection<TService, TImplementation>(this IServiceCollection services, string? streamName = null)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddTransient<TService, TImplementation>();
        services.AddLiveProjection(typeof(TService), streamName);
        return services;
    }

    public static IServiceCollection AddConstantReaction<T>(this IServiceCollection services, Func<IServiceProvider, T, Task> reaction)
    {
        services.AddSingleton(new ConstantReactionSource([new ConstantReaction((serviceProvider, obj) =>
        {
            if (obj is T t)
            {
                return reaction(serviceProvider, t);
            }
            return Task.CompletedTask;
        }, typeof(T))]));

        return services;
    }
    public static IServiceCollection AddConstantReactionClass<T>(this IServiceCollection services)
    {
        services.AddSingleton(serviceProvider =>
        {
            IAsyncHandlerProvider handlerProvider = serviceProvider.GetRequiredService<IAsyncHandlerProvider>();
            IEnumerable<AsyncHandler> handlers = handlerProvider.GetHandlerMethods(typeof(T));

            List<ConstantReaction> reactions = [];

            foreach (AsyncHandler handler in handlers)
            {
                ConstantReaction reaction = new(async (reactionServiceProvider, eventObject) =>
                {
                    object? instance = ActivatorUtilities.GetServiceOrCreateInstance(reactionServiceProvider, typeof(T));

                    await handler.Action.Invoke(instance, eventObject);

                }, handler.TargetType);

                reactions.Add(reaction);
            }

            return new ConstantReactionSource(reactions);
        });
        return services;
    }
    public static IServiceCollection AddConstantReactionService<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddSingleton<TService, TImplementation>();
        services.AddConstantReactionClass<TService>();
        return services;
    }
}
