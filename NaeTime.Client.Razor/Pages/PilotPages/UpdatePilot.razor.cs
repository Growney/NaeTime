using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Pages.PilotPages;
public partial class UpdatePilot : ComponentBase
{
    [Inject]
    private IPilotApiClient PilotApiClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid PilotId { get; set; }

    [Parameter]
    public string? ReturnUrl { get; set; }

    private Lib.Models.Pilot? _model = null;

    protected override async Task OnParametersSetAsync()
    {
        _model = await PilotApiClient.GetAsync(PilotId);
        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Lib.Models.Pilot pilot)
    {
        if (_model is null)
        {
            return;
        }

        await PilotApiClient.UpdateAsync(pilot);

        NavigationManager.NavigateTo(ReturnUrl ?? "/pilot/list");
    }
}
