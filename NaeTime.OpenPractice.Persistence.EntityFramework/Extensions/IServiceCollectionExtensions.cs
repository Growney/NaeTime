using NaeTime.OpenPractice.Persistence.EntityFramework;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkOpenPractice(this IServiceCollection services)
    {
        services.AddEventHub<ConsecutiveLapsLeaderboardService>();
        services.AddEventHub<OpenPracticeSessionService>();
        services.AddEventHub<SingleLapsLeaderboardService>();
        services.AddEventHub<TotalLapLeaderboardService>();
        services.AddEventHub<AverageLapLeaderboardService>();

        return services;
    }
}
