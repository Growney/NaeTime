using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddHttpClient(string.Empty, x =>
{
    x.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});
builder.Services.AddSpeechSynthesis();
builder.Services.AddApiClients();
builder.Services.AddEventDbSignalRReactions(sp =>
{
    NavigationManager navigationManager = sp.GetRequiredService<NavigationManager>();
    return navigationManager.BaseUri.TrimEnd('/');
});

await builder.Build().RunAsync();
