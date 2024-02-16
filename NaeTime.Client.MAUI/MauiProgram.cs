using Microsoft.Extensions.Logging;
using NaeTime.Client.MAUI.Lib;

namespace NaeTime.Client.MAUI;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddSingleton<ServiceRunner>();

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddClientPersistence<LocalStorageProvider>();
        builder.Services.AddNaeTimeComponents();
        builder.Services.AddAnnouncer<MauiSpeechProvider>();
        builder.Services.AddTimingCore();
        builder.Services.AddImmersionRCTiming();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddNaeTimePublishSubscribe();

        return builder.Build();
    }
}
