using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Abstractions;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Pages.Pilot;
public partial class NewPilot : ComponentBase
{
    [Inject]
    private ILocalApiClientProvider LocalApiClientProvider { get; set; } = null!;
    [Inject]
    private INavigationManager NavigationManager { get; set; } = null!;

    private readonly Lib.Models.Pilot _model = new();

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Lib.Models.Pilot pilot)
    {
        await LocalApiClientProvider.PilotApiClient.CreateAsync(pilot.FirstName, pilot.LastName, pilot.CallSign);

        NavigationManager.GoBack();
    }
}
