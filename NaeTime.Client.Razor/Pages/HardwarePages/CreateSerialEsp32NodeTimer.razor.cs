using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Orchestrator.Abstractions;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class CreateSerialEsp32NodeTimer
{
    [Inject]
    private INaeTimeOrchestrator Orchestrator { get; set; } = null!;
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

        await Orchestrator.Hardware.CreateSerialEsp32Node(timer.Name, timer.Port);
        await Orchestrator.CommitAsync();

        NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
    }
}
