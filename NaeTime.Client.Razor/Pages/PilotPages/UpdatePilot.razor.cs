using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Command.Abstractions;
using NaeTime.Query.Abstractions;
namespace NaeTime.Client.Razor.Pages.PilotPages;
public partial class UpdatePilot : ComponentBase
{
    [Inject]
    private IPilotCommandHandler PilotCommandHandler { get; set; } = null!;
    [Inject]
    private IPilotQueryHandler PilotQueryHandler { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid PilotId { get; set; }

    [Parameter]
    public string? ReturnUrl { get; set; }

    private Pilot? _model = null;

    protected override async Task OnParametersSetAsync()
    {
        var pilot = await PilotQueryHandler.GetPilotById(PilotId);

        if (pilot == null)
        {
            NavigationManager.NavigateTo(ReturnUrl ?? "/pilot/list");
            return;
        }

        _model = new()
        {
            Id = pilot.Id,
            FirstName = pilot.Firstname,
            LastName = pilot.Lastname,
            CallSign = pilot.Callsign,
        };

        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Pilot pilot)
    {
        if (_model is null)
        {
            return;
        }

        await PilotCommandHandler.RenamePilot(pilot.Id, pilot.FirstName, pilot.LastName);
        await PilotCommandHandler.ChangePilotCallsign(pilot.Id, pilot.CallSign);

        NavigationManager.NavigateTo(ReturnUrl ?? "/pilot/list");
    }
}
