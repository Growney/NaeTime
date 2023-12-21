using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Abstractions;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Pages.Hardware;
public partial class CreateEthernetLapRF8Channel : ComponentBase
{
    [Inject]
    private ILocalApiClientProvider LocalApiClientProvider { get; set; } = null!;
    [Inject]
    private INavigationManager NavigationManager { get; set; } = null!;

    private Lib.Models.EthernetLapRF8Channel? _model = new();

    private async Task HandleValidSubmit(Lib.Models.EthernetLapRF8Channel timer)
    {
        if (_model is null)
        {
            return;
        }

        await LocalApiClientProvider.HardwareApiClient.CreateEthernetLapRF8ChannelAsync(timer.Name, timer.IpAddress, timer.Port);

        NavigationManager.GoBack();
    }
}
