using NaeTime.OpenPractice;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddOpenPracticeCore(this IServiceCollection services)
    {
        services.AddEventHub<OpenPracticeConsecutiveLapsLeaderboardManager>();
        services.AddEventHub<OpenPracticeSingleLapLeaderboardManager>();
        services.AddEventHub<OpenPracticeTotalLapsLeaderboardManager>();
        services.AddEventHub<OpenPracticeAverageLapLeaderboardManager>();

        return services;
    }
}
