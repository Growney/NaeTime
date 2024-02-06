using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NaeTime.Persistence.Abstractions;

namespace NaeTime.Persistence.Extensions;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence<TRepositoryFactory>(this IServiceCollection services)
        where TRepositoryFactory : class, IRepositoryFactory
    {
        services.TryAddTransient<IRepositoryFactory, TRepositoryFactory>();
        services.AddSubscriberAssembly(typeof(HardwareService).Assembly);

        return services;
    }
}
