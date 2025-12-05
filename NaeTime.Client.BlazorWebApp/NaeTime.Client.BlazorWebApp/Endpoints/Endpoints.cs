namespace NaeTime.Client.BlazorWebApp.Endpoints;

public static class Endpoints
{
    public static IServiceCollection MapNaeTimeQueryEndpoints(WebApplication application)
    {
        application.MapHardwareQueryHandlerEndpoints();
        application.MapPilotQueryHandlerEndpoints();
        application.MapSessionQueryHandlerEndpoints();
        application.MapTrackQueryHandlerEndpoints();

        return application;
    }
}
