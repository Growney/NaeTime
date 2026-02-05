using MudBlazor;
using MudBlazor.Services;
using NaeTime.Client.BlazorWebApp.Components;
using NaeTime.Client.BlazorWebApp.Hubs;
using NaeTime.Client.BlazorWebApp.Services;
using NaeTime.Hardware.Abstractions;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using NaeTime.Client.BlazorWebApp;

var builder = WebApplication.CreateBuilder(args);

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
// Add services to the container.
builder.Services.AddSQLiteEventDbLite();
builder.Services.AddEventDbLiteSignalRServer();
builder.Services.AddNaeTimeQueries();
builder.Services.AddNaeTimeCommand();
builder.Services.AddHardwareCore();
builder.Services.AddNaeTimeEventReactions();
builder.Services.AddImmersionRCHardware();
builder.Services.AddEsp32NodeTimers();

builder.Services.AddSingleton<IRssiConsumer, RssiDistributionService>();

builder.Services.AddHostedService<ServerSpeech>();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
// Add COOP / COEP for multithreading
app.Use(async (context, next) =>
{
    context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
    context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
    await next();
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapHub<RssiHub>("/rssiHub");

app.MapEventDbLiteService();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(NaeTime.Client.BlazorWebApp.Client._Imports).Assembly);
app.MapNaeTimeEndPoints();

app.Run();
