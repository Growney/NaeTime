using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeRemoteProcedureCall<TManager>(this IServiceCollection services)
        where TManager : class, IRemoteProcedureCallRegistrar, IRemoteProcedureCallClient
    {
        services.TryAddSingleton(serviceProvider =>
        {
            IEnumerable<RemoteProcedureCallHubRegistration> hubRegistrations = serviceProvider.GetServices<RemoteProcedureCallHubRegistration>();

            TManager manager = ActivatorUtilities.CreateInstance<TManager>(serviceProvider);

            foreach (RemoteProcedureCallHubRegistration hubRegistration in hubRegistrations)
            {
                switch (hubRegistration.Lifetime)
                {
                    case HubLifetime.Service:
                        manager.RegisterServiceHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case HubLifetime.Scoped:
                        manager.RegisterScopedHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case HubLifetime.Transient:
                        manager.RegisterTransientHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case HubLifetime.Singleton:
                        object handler = hubRegistration.Instance ?? ActivatorUtilities.CreateInstance(serviceProvider, hubRegistration.HubType) ?? throw new InvalidOperationException($"Could not create an instance of {hubRegistration.HubType.Name}");
                        manager.RegisterHub(handler);
                        break;
                    default:
                        break;
                }
            }

            return manager;
        });
        services.TryAddSingleton<IRemoteProcedureCallRegistrar>(serviceProvider => serviceProvider.GetRequiredService<TManager>());
        services.TryAddSingleton<IRemoteProcedureCallClient>(serviceProvider => serviceProvider.GetRequiredService<TManager>());

        return services;
    }
    public static IServiceCollection AddRemoteProcedureCallHub(this IServiceCollection services, Type hubType, HubLifetime lifeTime = HubLifetime.Transient, object? instance = null)
    {
        services.AddSingleton(new RemoteProcedureCallHubRegistration(hubType, lifeTime, instance));
        return services;
    }
    public static IServiceCollection AddRemoteProcedureCallHub<THub>(this IServiceCollection services, HubLifetime lifeTime = HubLifetime.Transient, THub? instance = default)
    {
        services.AddSingleton(new RemoteProcedureCallHubRegistration(typeof(THub), lifeTime, instance));
        return services;
    }
    public static IServiceCollection AddNaeTimeEventing<TManager>(this IServiceCollection services)
        where TManager : class, IEventRegistrar, IEventClient
    {
        services.TryAddSingleton(serviceProvider =>
        {
            IEnumerable<EventHubRegistration> hubRegistrations = serviceProvider.GetServices<EventHubRegistration>();

            TManager manager = ActivatorUtilities.CreateInstance<TManager>(serviceProvider);

            foreach (EventHubRegistration hubRegistration in hubRegistrations)
            {
                switch (hubRegistration.Lifetime)
                {
                    case HubLifetime.Service:
                        manager.RegisterServiceHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case HubLifetime.Scoped:
                        manager.RegisterScopedHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case HubLifetime.Transient:
                        manager.RegisterTransientHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case HubLifetime.Singleton:
                        object handler = hubRegistration.Instance ?? ActivatorUtilities.CreateInstance(serviceProvider, hubRegistration.HubType) ?? throw new InvalidOperationException($"Could not create an instance of {hubRegistration.HubType.Name}");
                        manager.RegisterHub(handler);
                        break;
                    default:
                        break;
                }
            }

            return manager;
        });
        services.TryAddSingleton<IEventRegistrar>(x => x.GetRequiredService<TManager>());
        services.TryAddSingleton<IEventClient>(x => x.GetRequiredService<TManager>());
        services.TryAddTransient<IEventRegistrarScope>(x => x.GetRequiredService<TManager>().CreateScope());

        return services;
    }
    public static IServiceCollection AddEventHub(this IServiceCollection services, Type hubType, HubLifetime lifeTime = HubLifetime.Transient, object? instance = null)
    {
        services.AddSingleton(new EventHubRegistration(hubType, lifeTime, instance));
        return services;
    }
    public static IServiceCollection AddEventHub<THub>(this IServiceCollection services, HubLifetime lifeTime = HubLifetime.Transient, THub? instance = default)
    {
        services.AddSingleton(new EventHubRegistration(typeof(THub), lifeTime, instance));
        return services;
    }

    public static IServiceCollection AddEventAndRemoteProcedureCallHub(this IServiceCollection services, Type hubType, HubLifetime lifeTime = HubLifetime.Transient, object? instance = null)
    {
        services.AddSingleton(new RemoteProcedureCallHubRegistration(hubType, lifeTime, instance));
        services.AddSingleton(new EventHubRegistration(hubType, lifeTime, instance));
        return services;
    }

    public static IServiceCollection AddEventAndRemoteProcedureCallHub<THub>(this IServiceCollection services, HubLifetime lifeTime = HubLifetime.Transient, object? instance = null)
    {
        services.AddSingleton(new RemoteProcedureCallHubRegistration(typeof(THub), lifeTime, instance));
        services.AddSingleton(new EventHubRegistration(typeof(THub), lifeTime, instance));
        return services;
    }
}
