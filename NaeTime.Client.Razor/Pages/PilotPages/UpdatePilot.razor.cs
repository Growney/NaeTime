using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Management.Messages;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.PilotPages;
public partial class UpdatePilot : ComponentBase
{
    [Inject]
    private IRemoteProcedureCallClient RpcClient { get; set; } = null!;
    [Inject]
    private IEventClient EventClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid PilotId { get; set; }

    [Parameter]
    public string? ReturnUrl { get; set; }

    private Pilot? _model = null;

    protected override async Task OnParametersSetAsync()
    {
        Management.Messages.Models.Pilot? response = await RpcClient.InvokeAsync<Management.Messages.Models.Pilot>("GetPilot", PilotId);

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

        await EventClient.PublishAsync(new PilotDetailsChanged(pilot.Id, pilot.FirstName, pilot.LastName, pilot.CallSign));

        NavigationManager.NavigateTo(ReturnUrl ?? "/pilot/list");
    }
}
