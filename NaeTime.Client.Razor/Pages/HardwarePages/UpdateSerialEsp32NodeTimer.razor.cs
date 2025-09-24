using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Command.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class UpdateSerialEsp32NodeTimer
{
    [Inject]
    private INaeTimeNodeCommandHandler NaeTimeNodeCommandHandler { get; set; } = null!;
    [Inject]
    private IHardwareQueryHandler HardwareQueryHandler { get; set; } = null!;
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

        SerialNaeTimeNode? timer = await HardwareQueryHandler.GetSerialNaeTimeNode(TimerId);

        if (timer == null)
        {
            NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
            return;
        }

        _model = new SerialEsp32Node
        {
            Id = timer.Id,
            Name = timer.Name,
            Port = timer.Port
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

        await NaeTimeNodeCommandHandler.RenameDevice(TimerId, timer.Name);
        await NaeTimeNodeCommandHandler.ReconfigureSerialNode(TimerId, timer.Port);

        NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
    }
}
