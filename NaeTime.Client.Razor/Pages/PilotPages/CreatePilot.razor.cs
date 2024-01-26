using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Pages.PilotPages;
public partial class CreatePilot : ComponentBase
{
    [Inject]
    private IPilotApiClient PilotApiClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private readonly Lib.Models.Pilot _model = new(Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Lib.Models.Pilot pilot)
    {
        await PilotApiClient.CreateAsync(pilot.FirstName, pilot.LastName, pilot.CallSign);

        NavigationManager.NavigateTo(ReturnUrl ?? "/pilot/list");
    }
}
