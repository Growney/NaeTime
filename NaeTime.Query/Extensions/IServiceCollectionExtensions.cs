using NaeTime.Query;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Projections;
using NaeTime.Query.Projections.Abstractions;

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

        services.AddSingletonLiveProjection<IOpenPracticeProjection, OpenPracticeProjection>();
        services.AddSingletonLiveProjection<IDetectorProjection, DetectorProjection>();
        services.AddSingletonLiveProjection<IImmersionRCProjection, ImmersionRCProjection>();
        services.AddSingletonLiveProjection<IPilotProjection, PilotProjection>();
        services.AddSingletonLiveProjection<ITrackProjection, TrackProjection>();
        services.AddSingletonLiveProjection<ISessionProjection, SessionProjection>();
        services.AddSingletonLiveProjection<IOpenPracticeTimingProjection, OpenPracticeTimingProjection>();

        return services;
    }
}
