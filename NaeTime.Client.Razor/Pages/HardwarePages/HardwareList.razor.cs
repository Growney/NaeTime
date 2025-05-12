using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class HardwareList : ComponentBase
{
    [Inject]
    private IHardwareQueryHandler HardwareQueryHandler { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<TimerDetails> _timers = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var timers = await HardwareQueryHandler.GetAllDetectors();

        _timers.AddRange(timers.Select(x => new TimerDetails(x.Id, x.Name,
            x.Type switch
            {
                Query.Abstractions.Models.DetectorType.EthernetLapRF8Channel => TimerType.EthernetLapRF8Channel,
                DetectorType.NaeTimeSerial => TimerType.SerialEsp32Node,
                _ => throw new NotImplementedException()
            }, x.SupportedLanes)));

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
            case TimerType.SerialELRSBackpack:
                NavigationManager.NavigateTo($"/hardware/serialelrsbackpack/update/{details.Id}");
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
    private void NavigateToCreateSerialELRSBackpack()
    {
        NavigationManager.NavigateTo("/hardware/serialelrsbackpack/create");
    }
}
