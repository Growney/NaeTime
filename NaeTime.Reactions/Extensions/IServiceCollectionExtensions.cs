using NaeTime.Reactions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeEventReactions(this IServiceCollection services)
    {
        services.AddConstantReactionClass<DetectionReactions>();
        services.AddConstantReactionClass<OpenPracticeTimingReactions>();

        return services;
    }
}
