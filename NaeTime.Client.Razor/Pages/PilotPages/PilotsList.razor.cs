using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Orchestrator.Abstractions;

namespace NaeTime.Client.Razor.Pages.PilotPages;
public partial class PilotsList : ComponentBase
{
    [Inject]
    private INaeTimeOrchestrator Orchestrator { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        IEnumerable<Persistence.Abstractions.Management.Pilot>? pilotsResponse = await Orchestrator.Management.GetPilots();

        if (pilotsResponse == null)
        {
            return;
        }

        _pilots.AddRange(pilotsResponse.Select(x => new Pilot()
        {
            Id = x.Id,
            FirstName = x.FirstName,
            LastName = x.LastName,
            CallSign = x.CallSign,
        }));

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
