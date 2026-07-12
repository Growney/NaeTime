using Microsoft.Extensions.DependencyInjection;
using NaeTime.Client.BlazorWebApp.Client.Clients;
using NaeTime.Query.Abstractions;
using NaeTime.Command.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddApiClients(this IServiceCollection services)
    {
        // Query handlers
        services.AddScoped<IHardwareQueryHandler, HardwareQueryClient>();
        services.AddScoped<IOpenPracticeQueryHandler, OpenPracticeQueryClient>();
        services.AddScoped<IPilotQueryHandler, PilotQueryClient>();
        services.AddScoped<ISessionQueryHandler, SessionQueryClient>();
        services.AddScoped<ITrackQueryHandler, TrackQueryClient>();

        // Command handlers
        services.AddScoped<IImmersionRCLapRFCommandHandler, ImmersionRCCommandClient>();
        services.AddScoped<INaeTimeNodeCommandHandler, NaeTimeNodeCommandClient>();
        services.AddScoped<IOpenPracticeCommandHandler, OpenPracticeCommandClient>();
        services.AddScoped<IPilotCommandHandler, PilotCommandClient>();
        services.AddScoped<ISessionsCommandHandler, SessionsCommandClient>();
        services.AddScoped<ITrackCommandHandler, TrackCommandClient>();
        services.AddScoped<IELRSBackpackInterfaceCommandHandler, ELRSBackpackInterfaceCommandClient>();
        services.AddScoped<IDetectionCommandHandler, DetectionCommandClient>();

        return services;
    }
}
