using EventStore.Helpers;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionMethods
{
    public static IServiceCollection AddProjections(this IServiceCollection services, Action<IProjectionBuilder> configure)
    {
        services.AddSingleton(serviceProvider =>
        {
            var builder = ActivatorUtilities.CreateInstance<ProjectionHandlerBuilder>(serviceProvider);

            configure(builder);

            return builder;

        });
        services.AddSingleton<IProjectionBuilder>(serviceProvider => serviceProvider.GetRequiredService<ProjectionHandlerBuilder>());
        services.AddHostedService<ProjectionService>();

        return services;
    }
}

