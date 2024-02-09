namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeComponents(this IServiceCollection services)
    {
        services.AddBlazorBootstrap();
        return services;
    }
}
