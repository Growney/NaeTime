using NaeTime.Reactions;
using System.Threading.Channels;

namespace Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeEventReactions(this IServiceCollection services)
    {
        services.AddSingleton(_ => Channel.CreateUnbounded<EventEnvelope>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        }));
        services.AddSingleton<IEventChannel, EventChannel>();
        services.AddSingleton(sp => sp.GetRequiredService<Channel<EventEnvelope>>().Reader);
        services.AddSingleton(sp => sp.GetRequiredService<Channel<EventEnvelope>>().Writer);

        services.AddConstantReactionClass<OpenPracticeReactions>();
        services.AddConstantReactionClass<TimingReactions>();

        return services;
    }
}
