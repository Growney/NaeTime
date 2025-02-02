using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NaeTime.Hardware.Messages;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.HardwareComponents;
public partial class NodeTimerLaneTuner : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IEventRegistrarScope RegistrarScope { get; set; } = null!;
    [Inject]
    private IEventClient EventClient { get; set; } = null!;

    [Parameter]
    public int Width { get; set; } = 170;
    [Parameter]
    public int Height { get; set; } = 200;

    [Parameter]
    [EditorRequired]
    public Guid TimerId { get; set; }

    [Parameter]
    [EditorRequired]
    public byte LaneId { get; set; }

    private int chartId = 0;
    private IJSObjectReference? _chart;
    private IJSObjectReference? _rssiSeries;

    private int _entryThreshold = 40000;
    private int _exitThreshold = 40000;
    protected override Task OnInitializedAsync()
    {
        chartId = GetHashCode();

        return base.OnInitializedAsync();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _chart = await JSRuntime.InvokeAsync<IJSObjectReference>("createRssiSmootheChart", "rssi", chartId);
            _rssiSeries = await JSRuntime.InvokeAsync<IJSObjectReference>("addRssiLine", _chart);
            RegistrarScope.RegisterHub(this);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnValueChanged(int[] newValues)
    {

    }

    public async ValueTask DisposeAsync()
    {
        RegistrarScope?.Dispose();
        if (_rssiSeries != null)
        {
            await _rssiSeries.DisposeAsync();
        }

        if (_chart != null)
        {
            await _chart.DisposeAsync();
        }
    }

    public async Task When(RssiLevelRecorded rssiLevelRecorded)
    {
        if (_chart == null || _rssiSeries == null)
        {
            return;
        }

        if (rssiLevelRecorded.Lane != LaneId)
        {
            return;
        }

        await JSRuntime.InvokeVoidAsync("addValueToLine", _rssiSeries, DateTime.Now, rssiLevelRecorded.Level);
    }
    public Task SetEntryThreshold() => EventClient.PublishAsync(new NodeTimerEntryThresholdConfigured(TimerId, LaneId, (ushort)_entryThreshold));
    public Task SetExitThreshold() => EventClient.PublishAsync(new NodeTimerExitThresholdConfigured(TimerId, LaneId, (ushort)_exitThreshold));
}
