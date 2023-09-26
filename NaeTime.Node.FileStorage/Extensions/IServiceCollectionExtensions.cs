using NaeTime.Node.Abstractions.Repositories;
using NaeTime.Node.FileStorage;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddFileStorageNodeRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IUnitOfWork, NodeFileUnitOfWork>();
        services.AddSingleton<IConfigurationRepository>(x => x.GetRequiredService<NodeFileConfigurationRepository>());
        services.AddSingleton<NodeFileConfigurationRepository>();

        return services;
    }
}
