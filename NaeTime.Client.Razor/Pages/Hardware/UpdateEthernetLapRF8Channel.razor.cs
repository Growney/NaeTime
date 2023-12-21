using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Abstractions;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Pages.Hardware;
public partial class UpdateEthernetLapRF8Channel : ComponentBase
{
    [Inject]
    private ILocalApiClientProvider LocalApiClientProvider { get; set; } = null!;
    [Inject]
    private INavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid TimerId { get; set; }

    private EthernetLapRF8Channel? _model = new();

    protected override async Task OnParametersSetAsync()
    {
        _model = await LocalApiClientProvider.HardwareApiClient.GetEthernetLapRF8ChannelDetailsAsync(TimerId);
        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(EthernetLapRF8Channel timer)
    {
        if (_model is null)
        {
            return;
        }

        await LocalApiClientProvider.HardwareApiClient.UpdateEthernetLapRF8ChannelAsync(timer);

        NavigationManager.GoBack();
    }
}
