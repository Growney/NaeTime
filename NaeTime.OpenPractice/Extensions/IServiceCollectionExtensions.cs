using NaeTime.OpenPractice;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddOpenPracticeCore(this IServiceCollection services)
    {
        services.AddSubscriberAssembly(typeof(OpenPracticeSessionManager).Assembly);
        return services;
    }
}
