using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class UpdateEthernetLapRF8Channel : ComponentBase
{
    [Inject]
    private IHardwareApiClient HardwareApiClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid TimerId { get; set; }

    [Parameter]
    public string? ReturnUrl { get; set; }

    private EthernetLapRF8Channel? _model = null;

    protected override async Task OnParametersSetAsync()
    {
        _model = await HardwareApiClient.GetEthernetLapRF8ChannelDetailsAsync(TimerId);
        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(EthernetLapRF8Channel timer)
    {
        if (_model is null)
        {
            return;
        }

        await HardwareApiClient.UpdateEthernetLapRF8ChannelAsync(timer);

        NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
    }
}
