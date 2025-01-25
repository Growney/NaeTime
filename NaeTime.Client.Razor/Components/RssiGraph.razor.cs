using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.Hardware.Messages;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components;
public partial class RssiGraph : ComponentBase, IDisposable
{
    [Inject]
    private IEventRegistrarScope RegistrarScope { get; set; } = null!;

    [EditorRequired]
    [Parameter]
    public OpenPracticeLaneConfiguration Configuration { get; set; } = null!;

    [Parameter]
    public int Width { get; set; } = 170;
    [Parameter]
    public int Height { get; set; } = 200;

    private int chartId = 0;
    private IJSObjectReference? _chart;
    private IJSObjectReference? _rssiSeries;

    protected override Task OnInitializedAsync()
    {
        chartId = GetHashCode();

        return base.OnInitializedAsync();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _chart = await JSRuntime.InvokeAsync<IJSObjectReference>("createRssiSmootheChart", chartId);
        _rssiSeries = await JSRuntime.InvokeAsync<IJSObjectReference>("addRssiLine", _chart);

        await base.OnAfterRenderAsync(firstRender);
        RegistrarScope.RegisterHub(this);
    }

    public void Dispose() => RegistrarScope?.Dispose();

    public async Task When(RssiLevelRecorded rssiLevelRecorded)
    {
        if (_chart == null || _rssiSeries == null)
        {
            return;
        }

        if (rssiLevelRecorded.Lane != Configuration.LaneNumber)
        {
            return;
        }

        Configuration.RssiValue = rssiLevelRecorded.Level;
        if (Configuration.MaxRssiValue < rssiLevelRecorded.Level)
        {
            Configuration.MaxRssiValue = rssiLevelRecorded.Level;
        }

        await JSRuntime.InvokeVoidAsync("addValueToLine", _rssiSeries, DateTime.Now, rssiLevelRecorded.Level);
    }
}
