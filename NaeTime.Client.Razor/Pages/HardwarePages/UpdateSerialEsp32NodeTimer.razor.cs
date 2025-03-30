using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Persistence.Abstractions;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class UpdateSerialEsp32NodeTimer
{
    [Inject]
    private INaeTimePersistence Persistence { get; set; } = null!;
    [Inject]
    private INaeTimeOrchestrator Orchestrator { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid TimerId { get; set; }

    [Parameter]
    public string? ReturnUrl { get; set; }

    private SerialEsp32Node? _model = null;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnInitializedAsync();

        NaeTime.Persistence.Abstractions.Hardware.SerialEsp32Node? response = await Persistence.Hardware.GetSerialEsp32NodeTimer(TimerId);

        if (response == null)
        {
            return;
        }

        _model = new SerialEsp32Node
        {
            Id = response.TimerId,
            Name = response.Name,
            Port = response.Port
        };
    }

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

        await Orchestrator.Hardware.ConfigureSerialEsp32Node(timer.Id, timer.Name, timer.Port);
        await Orchestrator.CommitAsync();

        NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
    }
}
