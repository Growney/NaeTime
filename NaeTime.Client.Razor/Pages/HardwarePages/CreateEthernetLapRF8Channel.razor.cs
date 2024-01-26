using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class CreateEthernetLapRF8Channel : ComponentBase
{
    [Inject]
    private IHardwareApiClient HardwareApiClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private Lib.Models.EthernetLapRF8Channel? _model = new(Guid.NewGuid(), string.Empty, string.Empty, 5403);

    private async Task HandleValidSubmit(Lib.Models.EthernetLapRF8Channel timer)
    {
        if (_model is null)
        {
            return;
        }

        await HardwareApiClient.CreateEthernetLapRF8ChannelAsync(timer.Name, timer.IpAddress, timer.Port);

        NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
    }
}
