using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Query.Abstractions;

namespace NaeTime.Client.Razor.Pages.PilotPages;
public partial class PilotsList : ComponentBase
{
    [Inject]
    private IPilotQueryHandler PilotQueryHandler { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        IEnumerable<Query.Abstractions.Models.Pilot> pilots = await PilotQueryHandler.GetAllPilots();

        _pilots.AddRange(pilots.Select(x => new Pilot()
        {
            Id = x.Id,
            FirstName = x.Firstname,
            LastName = x.Lastname,
            CallSign = x.Callsign,
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
