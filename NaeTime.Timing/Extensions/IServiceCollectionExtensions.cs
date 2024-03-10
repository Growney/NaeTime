namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddTimingCore(this IServiceCollection services)
    {
        services.AddSubscriberAssembly(typeof(SoftwareTimer).Assembly);
        return services;
    }
}
