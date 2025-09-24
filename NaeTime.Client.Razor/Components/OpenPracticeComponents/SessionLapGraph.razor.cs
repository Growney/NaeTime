using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using System.Collections.Concurrent;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class SessionLapGraph
{
    [Parameter]
    [EditorRequired]
    public Guid SessionId { get; set; }

    private class LapTime
    {
        public Guid LapId { get; set; }
        public int LapNumber { get; set; }
        public double TotalSeconds { get; set; }
        public DateTime LapCompleted { get; set; }

    }

    private readonly ConcurrentDictionary<Guid, List<LapTime>> _lapTimes = new();
    private readonly List<Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        foreach (List<LapTime> pilotLapTimes in _lapTimes.Values)
        {
            SortAndMarkLapNumber(pilotLapTimes);
        }
    }


    private void SortAndMarkLapNumber(List<LapTime> times)
    {
        times.Sort((x, y) => x.LapCompleted.CompareTo(y.LapCompleted));

        for (int i = 0; i < times.Count; i++)
        {
            LapTime time = times[i];
            time.LapNumber = i + 1;
        }
    }
}