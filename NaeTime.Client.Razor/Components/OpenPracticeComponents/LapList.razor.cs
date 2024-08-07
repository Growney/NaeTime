using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class LapList : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public IEnumerable<OpenPracticeLap> Laps { get; set; } = Enumerable.Empty<OpenPracticeLap>();

    [Parameter]
    public bool IncludePilot { get; set; }
    [Parameter]
    public Guid SessionId { get; set; }
    [Parameter]
    public string? Header { get; set; }
    [Parameter]
    public IEnumerable<LapRecord> LapRecords { get; set; } = Enumerable.Empty<LapRecord>();


    [Inject]
    private IEventClient EventClient { get; set; } = null!;

    public Task Remove(Guid lapId, Guid pilotId) => EventClient.PublishAsync(new OpenPracticeLapRemoved(SessionId, lapId, pilotId));
    public Task Invalidate(Guid lapId, Guid pilotId)
    {
        OpenPracticeLap? lap = Laps.FirstOrDefault(l => l.Id == lapId);

        if (lap == null)
        {
            return Task.CompletedTask;
        }

        lap.Status = OpenPracticeLapStatus.Invalid;

        return EventClient.PublishAsync(new OpenPracticeLapDisputed(SessionId, lapId, pilotId, OpenPracticeLapDisputed.OpenPracticeLapStatus.Invalid));
    }
    public Task Validate(Guid lapId, Guid pilotId)
    {
        OpenPracticeLap? lap = Laps.FirstOrDefault(l => l.Id == lapId);

        if (lap == null)
        {
            return Task.CompletedTask;
        }

        lap.Status = OpenPracticeLapStatus.Completed;

        return EventClient.PublishAsync(new OpenPracticeLapDisputed(SessionId, lapId, pilotId, OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed));
    }

    private string GetStandardDeviation(int? topLaps)
    {
        IEnumerable<OpenPracticeLap> validLaps = Laps.Where(l => l.Status == OpenPracticeLapStatus.Completed).OrderBy(x => x.TotalMilliseconds);
        int lapCount = validLaps.Count();
        if (topLaps.HasValue)
        {
            int takeCount = (int)(topLaps.Value / 100.0 * lapCount);
            validLaps = validLaps.Take(takeCount);
        }

        if (validLaps.Count() <= 2)
        {
            return string.Empty;
        }

        double mean = validLaps.Average(l => l.TotalMilliseconds);
        double sumOfSquaresOfDifferences = validLaps.Sum(l => (l.TotalMilliseconds - mean) * (l.TotalMilliseconds - mean));
        double standardDeviation = (float)Math.Sqrt(sumOfSquaresOfDifferences / (lapCount - 1));

        string prefix = topLaps.HasValue ? $"std{topLaps}:" : "std:";

        return $"{prefix} {TimeSpan.FromMilliseconds(standardDeviation).TotalSeconds:0.00}";
    }
}
