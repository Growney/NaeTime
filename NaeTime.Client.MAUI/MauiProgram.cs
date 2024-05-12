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

        builder.Services.AddNaeTimeEventing();
        builder.Services.AddNaeTimeRemoteProcedureCall();

        //Must add all the SQLite services first so that the service runner creates the databases before the other services start
        builder.Services.AddSQLiteManagement();
        builder.Services.AddSQLiteHardware();
        builder.Services.AddSQLiteTiming();
        builder.Services.AddSQLiteOpenPractice();

        //Add Client Configuration Services
        builder.Services.AddLocalClientConfiguration<LocalStorageProvider>();

        //Add Announcer Services
        builder.Services.AddAnnouncer<MauiSpeechProvider>();

        //Add Hardware Services
        builder.Services.AddHardwareCore();
        builder.Services.AddImmersionRCHardware();

        //Add Management Services

        //Add Open Practice Services
        builder.Services.AddOpenPracticeCore();

        //Add Timing Services
        builder.Services.AddTimingCore();

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddNaeTimeComponents();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
