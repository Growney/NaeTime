using EventDbLite;
using EventDbLite.Abstractions;
using EventDbLite.Aggregates;
using EventDbLite.Handlers;
using EventDbLite.Handlers.Abstractions;
using EventDbLite.Projections;
using EventDbLite.Reactions;
using EventDbLite.Reactions.Abstractions;
using EventDbLite.Serialization;
using EventDbLite.Streams;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{

    public static IServiceCollection AddEventDbLite(this IServiceCollection services)
    {
        services.AddHostedService<ConstantReactionService>();

        services.AddSingleton<IEventStoreLite, EventStoreLite>();
        services.AddSingleton<ILiveProjectionRepository, LiveProjectionRepository>();

        services.AddSingleton<IEventSerializer, JsonEventSerializer>();
        services.AddSingleton<IHandlerProvider, HandlerProvider>();
        services.AddSingleton<IAsyncHandlerProvider, AsyncHandlerProvider>();

        services.AddTransient<IAggregateRepository, AggregateRepository>();
        services.AddTransient<IProjectionProvider, ProjectionProvider>();

        services.AddTransient<IStreamEventWriter, StreamEventWriter>();
        services.AddTransient<IReactionProviderFactory, ReactionProviderFactory>();
        services.AddTransient<IReactionClassFactory, ReactionClassFactory>();

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

    public static IServiceCollection AddConstantReaction<T>(this IServiceCollection services, Func<IServiceProvider, T, Task> reaction, string? reactionKey = null)
    {
        services.AddSingleton(new ConstantReactionSource([new ConstantReaction((serviceProvider, obj) =>
        {
            if (obj is T t)
            {
                return reaction(serviceProvider, t);
            }
            return Task.CompletedTask;
        }, typeof(T))], reactionKey));

        return services;
    }
    public static IServiceCollection AddConstantReactionClass<T>(this IServiceCollection services)
    {
        services.AddSingleton(serviceProvider =>
        {
            List<ConstantReaction> reactions = GetReactions<T>(serviceProvider);
            return new ConstantReactionSource(reactions, typeof(T).Name);
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
    public static IServiceCollection AddConstantReactionClass<T>(this IServiceCollection services, string? reactionKey)
    {
        services.AddSingleton(serviceProvider =>
        {
            List<ConstantReaction> reactions = GetReactions<T>(serviceProvider);
            return new ConstantReactionSource(reactions, reactionKey);
        });
        return services;
    }
    public static IServiceCollection AddConstantReactionService<TService, TImplementation>(this IServiceCollection services, string? reactionKey)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddSingleton<TService, TImplementation>();
        services.AddConstantReactionClass<TService>(reactionKey);
        return services;
    }

    private static List<ConstantReaction> GetReactions<T>(IServiceProvider serviceProvider)
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
        return reactions;
    }
}
