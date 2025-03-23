using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Persistence.Abstractions;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class HardwareList : ComponentBase
{
    [Inject]
    private INaeTimePersistence Persistence { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<TimerDetails> _timers = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        IEnumerable<NaeTime.Persistence.Abstractions.Hardware.TimerDetails>? timersResponse = await Persistence.Hardware.GetAllTimerDetails();

        if (timersResponse == null)
        {
            return;
        }

        _timers.AddRange(timersResponse.Select(x => new TimerDetails(x.Id, x.Name,
            x.Type switch
            {
                NaeTime.Persistence.Abstractions.Hardware.TimerType.EthernetLapRF8Channel => TimerType.EthernetLapRF8Channel,
                NaeTime.Persistence.Abstractions.Hardware.TimerType.SerialEsp32Node => TimerType.SerialEsp32Node,
                _ => throw new NotImplementedException()
            }, x.MaxLanes)));

    }

    private void NavigateToTimerDetails(TimerDetails details)
    {
        switch (details.Type)
        {
            case TimerType.EthernetLapRF8Channel:
                NavigationManager.NavigateTo($"/hardware/ethernetlaprf8channel/update/{details.Id}");
                break;
            case TimerType.SerialEsp32Node:
                NavigationManager.NavigateTo($"/hardware/serialesp32node/update/{details.Id}");
                break;
            default:
                break;
        }
    }
    private void NavigateToCreateLapRF8Channel()
    {
        NavigationManager.NavigateTo("/hardware/ethernetlaprf8channel/create");
    }
    private void NavigateToCreateSerialEsp32Node()
    {
        NavigationManager.NavigateTo("/hardware/serialesp32node/create");
    }
}
