using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using NaeTime.Client.MAUI.Lib;
using NaeTime.Hardware.Node.Esp32.Extensions;
using Syncfusion.Blazor;

namespace NaeTime.Client.MAUI;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        ServiceRunner? runner = null;

        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            .ConfigureLifecycleEvents(x =>
            {
#if WINDOWS
                x.AddWindows(windows =>
                {
                    windows.OnClosed(async (window, args) => 
                    {
                        try
                        {
                            if(runner != null)
                            {
                                await runner.DisposeAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle exception
                        }
                        finally
                        {
                        }
                    });
                });
#endif
            });

        builder.Services.AddSyncfusionBlazor();

        builder.Services.AddSingleton<ServiceRunner>(serviceProvider =>
        {
            runner = new ServiceRunner(serviceProvider.GetServices<IHostedService>());
            return runner;
        });

        //Add Client Configuration Services
        builder.Services.AddLocalClientConfiguration<LocalStorageProvider>();

        //Add Announcer Services
        builder.Services.AddAnnouncer<MauiSpeechProvider>();

        //Add Hardware Services
        builder.Services.AddHardwareCore();
        builder.Services.AddImmersionRCHardware();
        builder.Services.AddEsp32NodeTimers();
        builder.Services.AddBackpack();
        //Add Management Services


        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddNaeTimeComponents();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzI4NjkwMUAzMjM1MmUzMDJlMzBtTkNoaDhDTUtwTlRNMU9sNDZEM0FYUFNxSnZ3Rm5oMDVROHhQb2tSZU5ZPQ==;MzI4NjkwMkAzMjM1MmUzMDJlMzBSNUIxZUxGQmdxS0RzaUp3VGNMTk8xTjFoL2pMS2toeDhROHZjMnVsaWRnPQ==");

        return builder.Build();
    }
}
