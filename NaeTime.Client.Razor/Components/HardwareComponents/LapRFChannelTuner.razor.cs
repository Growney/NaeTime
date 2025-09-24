using Microsoft.AspNetCore.Components;
using NaeTime.Hardware.Messages;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.LinearGauge;

namespace NaeTime.Client.Razor.Components.HardwareComponents;
public partial class LapRFChannelTuner
{
    [Parameter]
    [EditorRequired]
    public Guid TimerId { get; set; }
    [Parameter]
    [EditorRequired]
    public byte Lane { get; set; }

    [Parameter]
    public byte? BandId { get; set; }
    [Parameter]
    public int? Frequency { get; set; }

    [Parameter]
    public bool IsEnabled { get; set; }
    [Parameter]
    public ushort? Gain { get; set; }
    [Parameter]
    public float? Threshold { get; set; }

    private float _maxRssi = 3000;
    private float _maxRecordedRssi = 0;
    private float _currentRssi = 0;

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }
    public async void UpdateThreshold(ValueChangeEventArgs args)
    {
        Threshold = (float)args.Value;
    }
    public Task EnabledChanged(bool value)
    {
        if (IsEnabled == value)
        {
            return Task.CompletedTask;
        }

        IsEnabled = value;

        return Task.CompletedTask;
    }
    public async void UpdateGain(SliderChangeEventArgs<ushort?> args)
    {
        Gain = args.Value ?? 0;
    }
    public async Task When(RssiLevelRecorded recorded)
    {
        if (TimerId != recorded.TimerId)
        {
            return;
        }

        if (Lane != recorded.Lane)
        {
            return;
        }

        if (recorded.Level > _maxRssi)
        {
            _maxRssi = recorded.Level;
        }

        if (recorded.Level > _maxRecordedRssi)
        {
            _maxRecordedRssi = recorded.Level;
        }

        _currentRssi = recorded.Level;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
}