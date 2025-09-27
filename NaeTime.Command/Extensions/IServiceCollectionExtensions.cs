using NaeTime.Command;
using NaeTime.Command.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddNaeTimeCommand(this IServiceCollection services)
    {
        services.AddTransient<IImmersionRCLapRFCommandHandler, ImmersionRCLapRFCommandHandler>();
        services.AddTransient<INaeTimeNodeCommandHandler, NaeTimeNodeCommandHandler>();
        services.AddTransient<IOpenPracticeCommandHandler, OpenPracticeCommandHandler>();
        services.AddTransient<IPilotCommandHandler, PilotCommandHandler>();
        services.AddTransient<ITrackCommandHandler, TrackCommandHandler>();
        return services;
    }
}
