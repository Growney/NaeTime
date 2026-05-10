
using NaeTime.Client.BlazorWebApp.Endpoints;

namespace Microsoft.AspNetCore.Builder;

public static class Endpoints
{
    public static WebApplication MapNaeTimeEndPoints(this WebApplication application)
    {
        application.MapHardwareQueryHandlerEndpoints();
        application.MapImmersionRCCommandHandlerEndpoints();
        application.MapNaeTimeNodeCommandHandlerEndpoints();
        application.MapOpenPracticeCommandHandlerEndpoints();
        application.MapOpenPracticeQueryHandlerEndpoints();
        application.MapPilotCommandHandlerEndpoints();
        application.MapPilotQueryHandlerEndpoints();
        application.MapSessionsCommandHandlerEndpoints();
        application.MapSessionQueryHandlerEndpoints();
        application.MapTrackCommandHandlerEndpoints();
        application.MapTrackQueryHandlerEndpoints();
        application.MapELRSBackpackInterfaceCommandHandlerEndpoints();

        return application;
    }
}
