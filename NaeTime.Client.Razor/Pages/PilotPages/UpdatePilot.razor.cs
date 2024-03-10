using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Management.Messages.Messages;
using NaeTime.Management.Messages.Requests;
using NaeTime.Management.Messages.Responses;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.PilotPages;
public partial class UpdatePilot : ComponentBase
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid PilotId { get; set; }

    [Parameter]
    public string? ReturnUrl { get; set; }

    private Pilot? _model = null;

    protected override async Task OnParametersSetAsync()
    {
        var response = await Dispatcher.Request<PilotRequest, PilotResponse>(new(PilotId));

        if (response == null)
        {
            return;
        }

        _model = new()
        {
            Id = response.Id,
            FirstName = response.FirstName,
            LastName = response.LastName,
            CallSign = response.CallSign,
        };

        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Pilot pilot)
    {
        if (_model is null)
        {
            return;
        }

        await Dispatcher.Dispatch(new PilotDetailsChanged(pilot.Id, pilot.FirstName, pilot.LastName, pilot.CallSign));

        NavigationManager.NavigateTo(ReturnUrl ?? "/pilot/list");
    }
}
