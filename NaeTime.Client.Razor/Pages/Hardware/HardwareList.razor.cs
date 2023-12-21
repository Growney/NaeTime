using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Abstractions;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Pages.Hardware;
public partial class HardwareList : ComponentBase
{
    [Inject]
    private ILocalApiClientProvider ClientProvider { get; set; } = null!;
    [Inject]
    private INavigationManager NavigationManager { get; set; } = null!;

    private readonly List<TimerDetails> _timers = new();

    protected override async Task OnInitializedAsync()
    {
        var existingTimers = await ClientProvider.HardwareApiClient.GetAllTimerDetailsAsync();

        _timers.AddRange(existingTimers);

        await base.OnInitializedAsync();
    }

    private void NavigateToTimerDetails(TimerDetails details)
    {
        switch (details.Type)
        {
            case TimerType.EthernetLapRF8Channel:
                NavigationManager.NavigateTo($"/hardware/ethernetlaprf8channel/update/{details.Id}");
                break;
            default:
                break;
        }
    }
}
