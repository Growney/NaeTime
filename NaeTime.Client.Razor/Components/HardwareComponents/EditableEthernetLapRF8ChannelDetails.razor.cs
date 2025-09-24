using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Hardware.Messages.Models;

namespace NaeTime.Client.Razor.Components.HardwareComponents;
public partial class EditableEthernetLapRF8ChannelDetails : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public Func<EthernetLapRF8Channel, Task> OnValidSubmit { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EthernetLapRF8Channel Details { get; set; } = null!;

    private readonly Dictionary<byte, LapRFLaneConfiguration> _laneConfigurations = new();

    private EditContext? _editContext;
    private bool _isConnected;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        _editContext = new EditContext(Details);
        base.OnParametersSet();
    }
    private Task HandleValidSubmit() => OnValidSubmit?.Invoke(Details) ?? Task.CompletedTask;
}