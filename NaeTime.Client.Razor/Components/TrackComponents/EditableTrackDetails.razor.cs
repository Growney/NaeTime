using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Components.TrackComponents;
public partial class EditableTrackDetails : ComponentBase
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
        Timers.Where(t => !Details.Timers.Any(g => g == t.Id));

    private void AddGate()
    {
        if (_selectedTimer == Guid.Empty)
        {
            return;
        }

        Details.AddTimer(_selectedTimer);
    }

    private string? GetTimerName(Guid timerId) => Timers.FirstOrDefault(t => t.Id == timerId)?.Name;

    private bool CanMoveUp(Guid timerId) => Details.CanTimerMoveUp(timerId);
    private bool CanMoveDown(Guid timerId) => Details.CanTimerMoveDown(timerId);

    private void MoveUp(Guid timerId) => Details.MoveTimerUp(timerId);
    private void MoveDown(Guid timerId) => Details.MoveTimerDown(timerId);
    private void RemoveGate(Guid timerId) => Details.RemoveTimer(timerId);
}
