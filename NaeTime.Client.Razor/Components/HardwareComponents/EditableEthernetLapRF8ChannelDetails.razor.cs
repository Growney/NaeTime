using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Hardware.ImmersionRC.Models;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Hardware;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.HardwareComponents;
public partial class EditableEthernetLapRF8ChannelDetails : ComponentBase, IDisposable
{
    [Inject]
    private ILapRFManager LapRFManager { get; set; } = null!;

    [Inject]
    private IEventRegistrar EventRegistrar { get; set; } = null!;

    [Inject]
    private IDistributionReceiver Receiver { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public Func<EthernetLapRF8Channel, Task> OnValidSubmit { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EthernetLapRF8Channel Details { get; set; } = null!;

    private readonly Dictionary<byte, LapRFLaneConfiguration> _laneConfigurations = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private EditContext? _editContext;
    private bool _isConnected;

    protected override async Task OnInitializedAsync()
    {

        _ = Receiver.Process<TimerConnected>(_cancellationTokenSource.Token, When);
        _ = Receiver.Process<TimerDisconnected>(_cancellationTokenSource.Token, When);
        EventRegistrar.RegisterHub(this);

        IEnumerable<Hardware.ImmersionRC.Models.LapRFLaneConfiguration>? laneConfigurations = await LapRFManager.GetTimeLaneConfigurations(Details.Id);

        if (laneConfigurations != null)
        {
            foreach (LapRFLaneConfiguration laneConfiguration in laneConfigurations)
            {
                _laneConfigurations.Add(laneConfiguration.Lane, laneConfiguration);
            }
        }

        _isConnected = await LapRFManager.IsTimerConnected(Details.Id);

        await base.OnInitializedAsync();
    }

    public async Task When(TimerConnected connected)
    {
        _isConnected = connected.TimerId == Details.Id;

        await InvokeAsync(StateHasChanged);
    }
    public async Task When(TimerDisconnected connected)
    {
        if (connected.TimerId != Details.Id)
        {
            return;
        }

        _isConnected = false;

        await InvokeAsync(StateHasChanged);
    }

    protected override void OnParametersSet()
    {
        _editContext = new EditContext(Details);
        base.OnParametersSet();
    }
    private Task HandleValidSubmit() => OnValidSubmit?.Invoke(Details) ?? Task.CompletedTask;
    public void Dispose() => _cancellationTokenSource.Cancel();
}