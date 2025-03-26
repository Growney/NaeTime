using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Orchestrator.Abstractions;

namespace NaeTime.Client.Razor.Pages.PilotPages;
public partial class CreatePilot : ComponentBase
{
    [Inject]
    private INaeTimeOrchestrator Orchestrator { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private readonly Pilot _model = new()
    {
        Id = Guid.NewGuid(),
        FirstName = null,
        LastName = null,
        CallSign = null
    };

    private async Task HandleValidSubmit(Pilot pilot)
    {
        await Orchestrator.Management.CreatePilot(pilot.FirstName, pilot.LastName, pilot.CallSign);
        await Orchestrator.CommitAsync();

        NavigationManager.NavigateTo(ReturnUrl ?? "/pilot/list");
    }
}
