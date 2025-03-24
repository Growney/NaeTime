using NaeTime.OpenPractice;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddOpenPracticeCore(this IServiceCollection services)
    {
        services.AddEventHub<DetectionService>();
        services.AddEventHub<OpenPracticeConsecutiveLapsLeaderboardManager>();
        services.AddEventHub<OpenPracticeSessionManager>();
        services.AddEventHub<OpenPracticeSingleLapLeaderboardManager>();
        services.AddEventHub<OpenPracticeTotalLapsLeaderboardManager>();
        services.AddEventHub<OpenPracticeAverageLapLeaderboardManager>();

        return services;
    }
}
