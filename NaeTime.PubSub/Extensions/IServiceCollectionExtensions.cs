using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeRemoteProcedureCall(this IServiceCollection services)
    {
        services.TryAddSingleton(serviceProvider =>
        {
            IEnumerable<RemoteProcedureCallHubRegistration> hubRegistrations = serviceProvider.GetServices<RemoteProcedureCallHubRegistration>();

            RemoteProcedureCallManager manager = ActivatorUtilities.CreateInstance<RemoteProcedureCallManager>(serviceProvider);

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
        services.TryAddSingleton<IRemoteProcedureCallRegistrar>(serviceProvider => serviceProvider.GetRequiredService<RemoteProcedureCallManager>());
        services.TryAddSingleton<IRemoteProcedureCallClient>(serviceProvider => serviceProvider.GetRequiredService<RemoteProcedureCallManager>());

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
    public static IServiceCollection AddNaeTimeEventing(this IServiceCollection services)
    {
        services.TryAddSingleton(serviceProvider =>
        {
            IEnumerable<EventHubRegistration> hubRegistrations = serviceProvider.GetServices<EventHubRegistration>();

            EventManager manager = ActivatorUtilities.CreateInstance<EventManager>(serviceProvider);

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
        services.TryAddSingleton<IEventRegistrar>(x => x.GetRequiredService<EventManager>());
        services.TryAddSingleton<IEventClient>(x => x.GetRequiredService<EventManager>());
        services.TryAddTransient<IEventRegistrarScope>(x => x.GetRequiredService<EventManager>().CreateScope());

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
