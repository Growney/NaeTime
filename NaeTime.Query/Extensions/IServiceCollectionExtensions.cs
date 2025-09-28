using NaeTime.Query;
using NaeTime.Query.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeQueries(this IServiceCollection services)
    {
        services.AddTransient<IHardwareQueryHandler, HardwareQueryHandler>();
        services.AddTransient<IPilotQueryHandler, PilotQueryHandler>();
        services.AddTransient<ITrackQueryHandler, TrackQueryHandler>();
        services.AddTransient<IOpenPracticeQueryHandler, OpenPracticeQueryHandler>();
        services.AddTransient<ISessionQueryHandler, SessionQueryHandler>();
        return services;
    }
}
