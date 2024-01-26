using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Pages.PilotPages;
public partial class PilotsList : ComponentBase
{
    [Inject]
    private IPilotApiClient PilotApiClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<Lib.Models.Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        var existingPilots = await PilotApiClient.GetAllAsync();

        _pilots.AddRange(existingPilots);

        await base.OnInitializedAsync();
    }

    private void NavigateToPilot(Lib.Models.Pilot pilot)
    {
        NavigationManager.NavigateTo($"/pilot/update/{pilot.Id}");
    }
    private void NavigateToCreatePilot()
    {
        NavigationManager.NavigateTo($"/pilot/create");
    }
}
