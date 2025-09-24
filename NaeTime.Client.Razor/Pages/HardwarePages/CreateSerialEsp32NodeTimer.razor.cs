using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Command.Abstractions;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class CreateSerialEsp32NodeTimer
{
    [Inject]
    private INaeTimeNodeCommandHandler NaeTimeNodeCommandHandler { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private readonly SerialEsp32Node? _model = new()
    {
        Id = Guid.NewGuid(),
        Name = null,
        Port = "COM4",

    };

    private async Task HandleValidSubmit(SerialEsp32Node timer)
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
        await NaeTimeNodeCommandHandler.ConfigureSerialEsp32Node(timer.Id, timer.Name, timer.Port, 8);

        NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
    }
}
