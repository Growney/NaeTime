using NaeTime.OpenPractice.Persistence.EntityFramework;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkOpenPractice(this IServiceCollection services)
    {
        services.AddEventAndRemoteProcedureCallHub<ConsecutiveLapsLeaderboardService>();
        services.AddEventAndRemoteProcedureCallHub<OpenPracticeSessionService>();
        services.AddEventAndRemoteProcedureCallHub<SingleLapsLeaderboardService>();
        services.AddEventAndRemoteProcedureCallHub<TotalLapLeaderboardService>();
        services.AddEventAndRemoteProcedureCallHub<AverageLapLeaderboardService>();

        return services;
    }
}
