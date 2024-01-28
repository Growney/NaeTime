using ImmersionRC.LapRF.Abstractions;
using ImmersionRC.LapRF.Communication;
using ImmersionRC.LapRF.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace ImmersionRC.LapRF.Extensions;
public static class IServiceCollectionExtension
{
    public static IServiceCollection AddImmersionRCLapRF(this IServiceCollection services)
    {
        services.AddTransient<ILapRFCommunicationFactory, LapRFCommunicationFactory>();
        services.AddTransient<ILapRFProtocolFactory, LapRFProtocolFactory>();

        return services;
    }
}
