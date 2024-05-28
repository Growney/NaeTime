using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Hardware.Messages;
using NaeTime.PubSub.Abstractions;
using System.Net;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class UpdateEthernetLapRF8Channel : ComponentBase
{
    [Inject]
    private IRemoteProcedureCallClient RpcClient { get; set; } = null!;
    [Inject]
    private IEventClient EventClient { get; set; } = null!;
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

        Hardware.Messages.Models.EthernetLapRF8ChannelTimer? response = await RpcClient.InvokeAsync<Hardware.Messages.Models.EthernetLapRF8ChannelTimer?>("GetEthernetLapRF8ChannelTimer", TimerId);

        if (response == null)
        {
            return;
        }

        _model = new EthernetLapRF8Channel
        {
            Id = response.TimerId,
            Name = response.Name,
            IpAddress = response.IpAddress.ToString(),
            Port = response.Port
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


        await EventClient.Publish(new EthernetLapRF8ChannelConfigured(timer.Id, timer.Name, validIP, timer.Port));

        NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
    }
}
