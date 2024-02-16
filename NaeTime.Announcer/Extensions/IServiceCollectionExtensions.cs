using NaeTime.Announcer;
using NaeTime.Announcer.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAnnouncer<TSpeechProvider>(this IServiceCollection services)
        where TSpeechProvider : class, ISpeechProvider
    {
        services.AddSingleton<IAnnouncmentQueue, AnnouncementQueue>();
        services.AddSingleton<ISpeechProvider, TSpeechProvider>();
        services.AddHostedService<Announcer>();

        services.AddSubscriberAssembly(typeof(Announcer).Assembly);

        return services;
    }
}
