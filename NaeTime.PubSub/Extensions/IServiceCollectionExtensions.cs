using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.PubSub.Extensions;

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
                    case RemoteProcedureCallHubLifetime.Service:
                        manager.RegisterServiceHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case RemoteProcedureCallHubLifetime.Scoped:
                        manager.RegisterScopedHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case RemoteProcedureCallHubLifetime.Transient:
                        manager.RegisterTransientHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case RemoteProcedureCallHubLifetime.Singleton:
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
    public static IServiceCollection AddRemoteProcedureCallHub(this IServiceCollection services, Type hubType, RemoteProcedureCallHubLifetime lifeTime = RemoteProcedureCallHubLifetime.Transient, object? instance = null)
    {
        services.AddSingleton(new RemoteProcedureCallHubRegistration(hubType, lifeTime, instance));
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
                    case EventHubLifetime.Service:
                        manager.RegisterServiceHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case EventHubLifetime.Scoped:
                        manager.RegisterScopedHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case EventHubLifetime.Transient:
                        manager.RegisterTransientHub(hubRegistration.HubType, serviceProvider);
                        break;
                    case EventHubLifetime.Singleton:
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

        return services;
    }
    public static IServiceCollection AddEventHub(this IServiceCollection services, Type hubType, EventHubLifetime lifeTime = EventHubLifetime.Transient, object? instance = null)
    {
        services.AddSingleton(new EventHubRegistration(hubType, lifeTime, instance));
        return services;
    }
}
