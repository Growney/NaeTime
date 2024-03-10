using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Hardware.Messages.Messages;
using NaeTime.PubSub.Abstractions;
using System.Net;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class CreateEthernetLapRF8Channel : ComponentBase
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private readonly EthernetLapRF8Channel? _model = new()
    {
        Id = Guid.NewGuid(),
        Name = null,
        IpAddress = "192.168.28.5",
        Port = 5403,

    };

    private async Task HandleValidSubmit(EthernetLapRF8Channel timer)
    {
        if (_model is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(timer.Name))
        {
            return;
        }

        if (!IPAddress.TryParse(timer.IpAddress, out var validIP))
        {
            return;
        }

        await Dispatcher.Dispatch(new EthernetLapRF8ChannelConfigured(timer.Id, timer.Name, validIP, timer.Port));

        NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
    }
}
