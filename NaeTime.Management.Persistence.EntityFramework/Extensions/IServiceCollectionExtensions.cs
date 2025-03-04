using NaeTime.Management.Persistence.EntityFramework;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkManagement(this IServiceCollection services)
    {
        services.AddEventAndRemoteProcedureCallHub<PilotService>();
        services.AddEventAndRemoteProcedureCallHub<TrackService>();
        services.AddEventAndRemoteProcedureCallHub<ActiveService>();
        return services;
    }
}
