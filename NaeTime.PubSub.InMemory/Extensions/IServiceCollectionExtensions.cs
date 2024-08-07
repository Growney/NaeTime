using NaeTime.PubSub.InMemory;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeInMemoryPubSub(this IServiceCollection services)
    {
        services.AddNaeTimeRemoteProcedureCall<InMemoryRemoteProcedureCallManager>();
        services.AddNaeTimeEventing<InMemoryEventManager>();

        return services;
    }
}
