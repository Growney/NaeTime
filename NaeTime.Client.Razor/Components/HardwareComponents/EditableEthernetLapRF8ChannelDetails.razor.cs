using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Hardware.Messages;
using NaeTime.Hardware.Messages.Models;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.HardwareComponents;
public partial class EditableEthernetLapRF8ChannelDetails : ComponentBase
{
    [Inject]
    public IEventRegistrarScope EventRegistrar { get; set; } = null!;
    [Inject]
    public IRemoteProcedureCallClient RpcClient { get; set; } = null!;

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
        EventRegistrar.RegisterHub(this);

        IEnumerable<LapRFLaneConfiguration>? laneConfigurations = await RpcClient.InvokeAsync<IEnumerable<LapRFLaneConfiguration>>("GetEthernetLapRF8ChannelTimerLaneConfigurations", Details.Id);

        if (laneConfigurations != null)
        {
            foreach (LapRFLaneConfiguration laneConfiguration in laneConfigurations)
            {
                _laneConfigurations.Add(laneConfiguration.Lane, laneConfiguration);
            }
        }

        _isConnected = await RpcClient.InvokeAsync<bool>("IsEthernetLapRF8ChannelTimerConnected", Details.Id);

        await base.OnInitializedAsync();
    }

    public async void When(TimerConnectionEstablished connected)
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
}