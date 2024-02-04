using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class HardwareList : ComponentBase
{
    [Inject]
    private IPublisher Publisher { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<TimerDetails> _timers = new();

    protected override async Task OnInitializedAsync()
    {
        var existingTimers = await HardwareApiClient.GetAllTimerDetailsAsync();

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
    private void NavigateToCreateLapRF8Channel()
    {
        NavigationManager.NavigateTo("/hardware/ethernetlaprf8channel/create");
    }
}
