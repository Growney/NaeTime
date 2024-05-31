using NaeTime.OpenPractice;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddOpenPracticeCore(this IServiceCollection services)
    {
        services.AddEventAndRemoteProcedureCallHub<DetectionService>();
        services.AddEventAndRemoteProcedureCallHub<OpenPracticeConsecutiveLapsLeaderboardManager>();
        services.AddEventAndRemoteProcedureCallHub<OpenPracticeSessionManager>();
        services.AddEventAndRemoteProcedureCallHub<OpenPracticeSingleLapLeaderboardManager>();
        services.AddEventAndRemoteProcedureCallHub<OpenPracticeTotalLapsLeaderboardManager>();
        services.AddEventAndRemoteProcedureCallHub<OpenPracticeAverageLapLeaderboardManager>();

        return services;
    }
}
