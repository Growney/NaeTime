using MudBlazor;
using MudBlazor.Services;
using NaeTime.Client.BlazorWebApp.Components;
using Toolbelt.Blazor.Extensions.DependencyInjection;

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
builder.Services.AddEventDbLite();
builder.Services.AddEventDbLiteSignalRServer();
builder.Services.AddEventDbSignalRReactions("http://localhost:5118");
builder.Services.AddNaeTimeQueries();
builder.Services.AddNaeTimeCommand();
builder.Services.AddHardwareCore();
builder.Services.AddNaeTimeEventReactions();
builder.Services.AddImmersionRCHardware();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
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
app.UseAntiforgery();

app.MapStaticAssets();

app.MapEventDbLiteService();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(NaeTime.Client.BlazorWebApp.Client._Imports).Assembly);

app.Run();
