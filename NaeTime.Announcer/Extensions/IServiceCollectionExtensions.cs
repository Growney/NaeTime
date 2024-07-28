using NaeTime.Announcer;
using NaeTime.Announcer.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAnnouncer<TSpeechProvider>(this IServiceCollection services)
        where TSpeechProvider : class, ISpeechProvider
    {
        services.AddSingleton<ISpeechProvider, TSpeechProvider>();
        services.AddHostedService<Announcer>();

        services.AddSingleton<OpenPracticeLapAnnouncer>();
        services.AddSingleton<IAnnouncmentProvider>(serviceProvider => serviceProvider.GetRequiredService<OpenPracticeLapAnnouncer>());
        services.AddEventAndRemoteProcedureCallHub<OpenPracticeLapAnnouncer>(NaeTime.PubSub.HubLifetime.Service);

        services.AddEventAndRemoteProcedureCallHub<HardwareAnnouncerService>(NaeTime.PubSub.HubLifetime.Singleton);

        return services;
    }
}
