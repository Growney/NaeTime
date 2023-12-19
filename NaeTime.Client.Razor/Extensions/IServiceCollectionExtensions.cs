using NaeTime.Client.Razor.Abstractions;
using NaeTime.Client.Razor.Lib;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeComponents(this IServiceCollection services)
    {
        services.AddScoped<INavigationManager, StackedNavigationManager>();

        return services;
    }
}
