using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Messages.Events.Entities;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.PilotPages;
public partial class CreatePilot : ComponentBase
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
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

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Pilot pilot)
    {
        await Dispatcher.Dispatch(new PilotCreated(pilot.Id, pilot.FirstName, pilot.LastName, pilot.CallSign));

        NavigationManager.NavigateTo(ReturnUrl ?? "/pilot/list");
    }
}
