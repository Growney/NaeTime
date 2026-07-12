using EventDbLite;
using EventDbLite.Aggregates;
using NaeTime.Events.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NaeTime.Command.Aggregates;

public class OpenPracticePilotTiming : AggregateRoot<OpenPracticePilotTiming.PilotKey>
{
    public record PilotKey(Guid OpenPracticeSessionId, Guid PilotId)
    {
        public override string ToString() => $"{OpenPracticeSessionId}-{PilotId}";
    }

    private TimeSpan? _minimumLapTime;
    private TimeSpan? _maximumLapTime;

    private HashSet<Guid> _packEnds = new();
    public OpenPracticePilotTiming()
    {

    }
    public OpenPracticePilotTiming(Guid openPracticeSessionId, Guid pilotId)
    {
        Raise(new OpenPracticePilotTimingStarted(openPracticeSessionId, pilotId));
    }
    private void When(OpenPracticePilotTimingStarted started)
    {
        Id = new PilotKey(started.SessionId, started.PilotId);
    }
    private void When(OpenPracticeSessionPilotMinimumLapTimeSet set)
    {
        _minimumLapTime = set.MinimumLapTime;
    }
    private void When(OpenPracticeSessionPilotMinimumLapTimeReset _)
    {
        _minimumLapTime = null;
    }
    private void When(OpenPracticeSessionPilotMaximumLapTimeSet set)
    {
        _maximumLapTime = set.MaximumLapTime;
    }
    private void When(OpenPracticeSessionPilotMaximumLapTimeReset _)
    {
        _maximumLapTime = null;
    }
    public void ResetMinimumLapTime()
    {
        ThrowIfIdNotSet();
        if (_minimumLapTime is null)
        {
            return;
        }
        Raise(new OpenPracticeSessionPilotMinimumLapTimeReset(Id.OpenPracticeSessionId, Id.PilotId));
    }
    public void SetMinimumLapTime(TimeSpan minimumLapTime)
    {
        ThrowIfIdNotSet();
        if (minimumLapTime <= TimeSpan.Zero)
        {
            throw new ValidationException("Minimum lap time must be greater than zero.");
        }
        if (minimumLapTime == _minimumLapTime)
        {
            return;
        }
        Raise(new OpenPracticeSessionPilotMinimumLapTimeSet(Id.OpenPracticeSessionId, Id.PilotId, minimumLapTime));
    }
    public void ResetMaximumLapTime()
    {
        ThrowIfIdNotSet();
        if (_maximumLapTime is null)
        {
            return;
        }
        Raise(new OpenPracticeSessionPilotMaximumLapTimeReset(Id.OpenPracticeSessionId, Id.PilotId));
    }
    public void SetMaximumLapTime(TimeSpan maximumLapTime)
    {
        ThrowIfIdNotSet();
        if (maximumLapTime <= TimeSpan.Zero)
        {
            throw new ValidationException("Maximum lap time must be greater than zero.");
        }
        if (maximumLapTime == _maximumLapTime)
        {
            return;
        }
        Raise(new OpenPracticeSessionPilotMaximumLapTimeSet(Id.OpenPracticeSessionId, Id.PilotId, maximumLapTime));
    }
    public void MarkEndOfPilotPack(Guid packEndId, long softwareTime, DateTime utcTime)
    {
        ThrowIfIdNotSet();
        if (_packEnds.Contains(packEndId))
        {
            throw new InvalidOperationException("Packend already exists");
        }

        Raise(new OpenPracticePilotPackEndAdded(Id.OpenPracticeSessionId, Id.PilotId, packEndId, softwareTime, utcTime));
    }
    public void RemoveEndOfPilotPack(Guid packEndId)
    {
        ThrowIfIdNotSet();
        if (!_packEnds.Contains(packEndId))
        {
            return;
        }

        Raise(new OpenPracticePilotPackEndRemoved(Id.OpenPracticeSessionId, Id.PilotId, packEndId));
    }
    private void When(OpenPracticePilotPackEndAdded packEnd)
    {
        _packEnds.Add(packEnd.PackEndId);
    }
}
