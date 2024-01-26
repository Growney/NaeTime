using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Components.TrackComponents;
public partial class EditableTrackDetails
{
    [Parameter]
    [EditorRequired]
    public Func<Track, Task> OnValidSubmit { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public Track Details { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public IEnumerable<TimerDetails> Timers { get; set; } = Enumerable.Empty<TimerDetails>();

    private Guid _selectedTimer;

    private EditContext? _editContext;

    protected override void OnParametersSet()
    {
        _editContext = new EditContext(Details);
        base.OnParametersSet();
    }

    private Task HandleValidSubmit()
    {
        return OnValidSubmit?.Invoke(Details) ?? Task.CompletedTask;
    }

    private IEnumerable<TimerDetails> GetAvailableTimers() =>
        Timers.Where(t => !Details.TimedGates.Any(g => g.TimerId == t.Id));

    private void AddGate()
    {
        if (_selectedTimer == Guid.Empty)
        {
            return;
        }

        Details.AddTimedGate(_selectedTimer);
    }

    private string? GetTimerName(Guid timerId) => Timers.FirstOrDefault(t => t.Id == timerId)?.Name;

    private bool CanMoveUp(TimedGate gate) => Details.CanTimedGateMoveUp(gate.TimerId);
    private bool CanMoveDown(TimedGate gate) => Details.CanTimedGateMoveDown(gate.TimerId);

    private void MoveUp(TimedGate gate) => Details.MoveTimedGateUp(gate.TimerId);
    private void MoveDown(TimedGate gate) => Details.MoveTimedGateDown(gate.TimerId);
    private void RemoveGate(TimedGate gate) => Details.RemoveTimedGate(gate.TimerId);
}
