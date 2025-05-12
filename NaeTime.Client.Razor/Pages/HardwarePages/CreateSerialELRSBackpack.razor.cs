using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Hardware.Messages;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class CreateSerialELRSBackpack
{
    [Inject]
    private IEventClient EventClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private readonly SerialELRSBackpack? _model = new()
    {
        Id = Guid.NewGuid(),
        Name = null,
        Port = "COM4",

    };

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
