using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NaeTime.Hardware.Messages;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.HardwareComponents;
public partial class NodeTimerLaneTuner : ComponentBase, IAsyncDisposable
{
    private const float _thresholdLineThinkness = 2f;
    private const float _unsetThresholdLineThinkness = 1f;
    private const string _entryThresholdHorizontalLineColour = "#198754";
    private const string _exitThresholdHorizontalLineColour = "#dc3545";
    private const string _unsetEntryThresholdHorizontalLineColour = "#3ddb92";
    private const string _unsetExitThresholdHorizontalLineColour = "#e87d88";

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

    private IJSObjectReference? _entryThresholdLine;
    private IJSObjectReference? _exitThresholdLine;
    private IJSObjectReference? _unsetEntryThresholdLine;
    private IJSObjectReference? _unsetExitThresholdLine;

    private int _entryThreshold = 28000;
    private int _exitThreshold = 28000;

    private int _configuredEntryThreshold;
    private int _configuredExitThreshold;
    protected override Task OnInitializedAsync()
    {
        chartId = GetHashCode();
        _configuredEntryThreshold = _entryThreshold;
        _configuredExitThreshold = _exitThreshold;

        return base.OnInitializedAsync();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {

            _chart = await JSRuntime.InvokeAsync<IJSObjectReference>("createRssiSmootheChart", "rssi", chartId, 0, ushort.MaxValue / 2);
            _rssiSeries = await JSRuntime.InvokeAsync<IJSObjectReference>("addRssiLine", _chart);

            _entryThresholdLine = await JSRuntime.InvokeAsync<IJSObjectReference>("addHorizontalLine", _chart, _entryThreshold, _entryThresholdHorizontalLineColour, _thresholdLineThinkness);
            _exitThresholdLine = await JSRuntime.InvokeAsync<IJSObjectReference>("addHorizontalLine", _chart, _exitThreshold, _exitThresholdHorizontalLineColour, _thresholdLineThinkness);
            _unsetEntryThresholdLine = await JSRuntime.InvokeAsync<IJSObjectReference>("addHorizontalLine", _chart, _entryThreshold, _entryThresholdHorizontalLineColour, _unsetThresholdLineThinkness);
            _unsetExitThresholdLine = await JSRuntime.InvokeAsync<IJSObjectReference>("addHorizontalLine", _chart, _entryThreshold, _exitThresholdHorizontalLineColour, _unsetThresholdLineThinkness);

            RegistrarScope.RegisterHub(this);
        }

        await base.OnAfterRenderAsync(firstRender);
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

    private async Task EntryThresholdValueChanged(int newValue)
    {
        if (_unsetEntryThresholdLine == null)
        {
            return;
        }

        _configuredEntryThreshold = newValue;
        await JSRuntime.InvokeVoidAsync("showHideHorizontalLine", _unsetEntryThresholdLine, _configuredEntryThreshold == _entryThreshold);
        if (_configuredEntryThreshold != _entryThreshold)
        {
            await JSRuntime.InvokeVoidAsync("setLineValue", _unsetEntryThresholdLine, _configuredEntryThreshold);
        }
    }
    public async Task SetEntryThreshold()
    {
        if (_entryThresholdLine == null || _unsetEntryThresholdLine == null)
        {
            return;
        }

        _entryThreshold = _configuredEntryThreshold;

        await JSRuntime.InvokeVoidAsync("setLineValue", _entryThresholdLine, _entryThreshold);
        await JSRuntime.InvokeVoidAsync("showHideHorizontalLine", _unsetEntryThresholdLine, false);
        await EventClient.PublishAsync(new NodeTimerEntryThresholdConfigured(TimerId, LaneId, (ushort)_entryThreshold));
    }

    private async Task ExitThresholdValueChanged(int newValue)
    {
        if (_unsetEntryThresholdLine == null)
        {
            return;
        }

        _configuredExitThreshold = newValue;
        await JSRuntime.InvokeVoidAsync("showHideHorizontalLine", _unsetExitThresholdLine, _configuredExitThreshold == _exitThreshold);
        if (_configuredExitThreshold != _exitThreshold)
        {
            await JSRuntime.InvokeVoidAsync("setLineValue", _unsetExitThresholdLine, _configuredExitThreshold);
        }
    }
    public async Task SetExitThreshold()
    {
        if (_exitThresholdLine == null || _unsetExitThresholdLine == null)
        {
            return;
        }

        _exitThreshold = _configuredExitThreshold;

        await JSRuntime.InvokeVoidAsync("setLineValue", _exitThresholdLine, _exitThreshold);
        await JSRuntime.InvokeVoidAsync("showHideHorizontalLine", _unsetExitThresholdLine, false);
        await EventClient.PublishAsync(new NodeTimerExitThresholdConfigured(TimerId, LaneId, (ushort)_exitThreshold));
    }
}
