using NaeTime.Management.Persistence.EntityFramework;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkManagement(this IServiceCollection services)
    {
        services.AddEventHub<PilotService>();
        services.AddEventHub<TrackService>();
        services.AddEventHub<ActiveService>();
        return services;
    }
}
