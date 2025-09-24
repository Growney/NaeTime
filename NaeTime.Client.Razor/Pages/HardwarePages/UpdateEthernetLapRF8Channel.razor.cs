using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Command.Abstractions;
using NaeTime.Query.Abstractions;
using System.Net;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class UpdateEthernetLapRF8Channel : ComponentBase
{
    [Inject]
    private IImmersionRCLapRFCommandHandler ImmersionRCLapRFCommandHandler { get; set; } = null!;
    [Inject]
    private IHardwareQueryHandler HardwareQueryHandler { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid TimerId { get; set; }

    [Parameter]
    public string? ReturnUrl { get; set; }

    private EthernetLapRF8Channel? _model = null;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnInitializedAsync();

        Query.Abstractions.Models.Ethernet8ChannelImmersionRCLapRF? timer = await HardwareQueryHandler.GetEthernet8ChannelImmersionRCLapRF(TimerId);


        if (timer == null)
        {
            NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
            return;
        }

        _model = new EthernetLapRF8Channel
        {
            Id = timer.Id,
            Name = timer.Name,
            IpAddress = timer.IPAddress.ToString(),
            Port = timer.Port
        };
    }

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

        if (!IPAddress.TryParse(timer.IpAddress, out IPAddress? validIP))
        {
            return;
        }

        await ImmersionRCLapRFCommandHandler.RenameDevice(TimerId, timer.Name);
        await ImmersionRCLapRFCommandHandler.ReconfigureNetworkDevice(TimerId, validIP, (ushort)timer.Port);

        NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
    }
}
