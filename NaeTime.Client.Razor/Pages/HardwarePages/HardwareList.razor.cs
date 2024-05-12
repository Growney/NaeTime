using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class HardwareList : ComponentBase
{
    [Inject]
    private IRemoteProcedureCallClient RpcClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private readonly List<TimerDetails> _timers = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var timersResponse = await RpcClient.InvokeAsync<IEnumerable<Hardware.Messages.Models.TimerDetails>>("GetAllTimerDetails");

        if (timersResponse == null)
        {
            return;
        }

        _timers.AddRange(timersResponse.Select(x => new TimerDetails(x.Id, x.Name,
            x.Type switch
            {
                Hardware.Messages.Models.TimerType.EthernetLapRF8Channel => TimerType.EthernetLapRF8Channel,
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
            default:
                break;
        }
    }
    private void NavigateToCreateLapRF8Channel()
    {
        NavigationManager.NavigateTo("/hardware/ethernetlaprf8channel/create");
    }
}
