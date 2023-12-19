using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Abstractions;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Pages.Pilot;
public partial class UpdatePilot : ComponentBase
{
    [Inject]
    private ILocalApiClientProvider LocalApiClientProvider { get; set; } = null!;
    [Inject]
    private INavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid PilotId { get; set; }

    private Lib.Models.Pilot? _model = new();

    protected override async Task OnParametersSetAsync()
    {
        _model = await LocalApiClientProvider.PilotApiClient.GetAsync(PilotId);
        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Lib.Models.Pilot pilot)
    {
        if (_model is null)
        {
            return;
        }

        await LocalApiClientProvider.PilotApiClient.UpdateAsync(pilot);

        NavigationManager.GoBack();
    }
}
