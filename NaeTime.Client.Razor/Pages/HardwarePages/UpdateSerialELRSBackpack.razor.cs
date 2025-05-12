using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Hardware.Messages;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class UpdateSerialELRSBackpack
{
    [Inject]
    private IRemoteProcedureCallClient RpcClient { get; set; } = null!;
    [Inject]
    private IEventClient EventClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    [Parameter]
    public string? ReturnUrl { get; set; }

    private SerialELRSBackpack? _model = null;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnInitializedAsync();

        Hardware.Messages.Models.SerialELRSBackpack? response = await RpcClient.InvokeAsync<Hardware.Messages.Models.SerialELRSBackpack?>("GetSerialELRSBackpack", Id);

        if (response == null)
        {
            return;
        }

        _model = new SerialELRSBackpack
        {
            Id = response.Id,
            Name = response.Name,
            Port = response.Port
        };
    }

    private async Task HandleValidSubmit(SerialELRSBackpack timer)
    {
        if (_model is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(timer.Name))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(timer.Port))
        {
            return;
        }


        await EventClient.PublishAsync(new SerialELRSBackpackConfigured(timer.Id, timer.Name, timer.Port));

        NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
    }
}
