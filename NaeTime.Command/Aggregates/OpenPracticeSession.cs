using EventDbLite.Aggregates;
using NaeTime.Events.Domain;
using NaeTime.Query.Abstractions.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
namespace NaeTime.Command.Aggregates;

public class OpenPracticeSession : AggregateRoot<Guid>
{
    private Guid _trackId;

    private TimeSpan? _minimumLapTime;
    private TimeSpan? _maximumLapTime;
    public OpenPracticeSession()
    {

    }
    public OpenPracticeSession(Guid id, Guid trackId, string name, TimeSpan? minimumLapTime, TimeSpan? maximumLapTime)
    {
        Raise(new OpenPracticeSessionScheduled(id, name, trackId, minimumLapTime, maximumLapTime));
    }

    public OpenPracticeSession(Guid oldId, Guid id, Guid trackId, string name, TimeSpan? minimumLapTime, TimeSpan? maximumLapTime)
        : this(id, trackId, name, minimumLapTime, maximumLapTime)
    {
        Raise(new OpenPracticeSessionCloneCreated(id, oldId, name));
    }

    public void Rename(string name)
    {
        Raise(new OpenPracticeSessionRenamed(Id, name));
    }

    private void When(OpenPracticeSessionScheduled scheduledEvent)
    {
        Id = scheduledEvent.SessionId;
        _trackId = scheduledEvent.TrackId;
    }

    private void When(OpenPracticeSessionMinimumLapTimeSet set)
    {
        _minimumLapTime = set.MinimumLapTime;
    }
    private void When(OpenPracticeSessionMinimumLapTimeReset _)
    {
        _minimumLapTime = null;
    }
    private void When(OpenPracticeSessionMaximumLapTimeSet set)
    {
        _maximumLapTime = set.MaximumLapTime;
    }
    private void When(OpenPracticeSessionMaximumLapTimeReset _)
    {
        _maximumLapTime = null;
    }

    public void ResetMinimumLapTime()
    {
        if (_minimumLapTime is null)
        {
            return;
        }
        Raise(new OpenPracticeSessionMinimumLapTimeReset(Id));
    }
    public void SetMinimumLapTime(TimeSpan minimumLapTime)
    {
        if (minimumLapTime <= TimeSpan.Zero)
        {
            throw new ValidationException("Minimum lap time must be greater than zero.");
        }
        if (minimumLapTime == _minimumLapTime)
        {
            return;
        }
        Raise(new OpenPracticeSessionMinimumLapTimeSet(Id, minimumLapTime));
    }
    public void ResetMaximumLapTime()
    {
        if (_maximumLapTime is null)
        {
            return;
        }
        Raise(new OpenPracticeSessionMaximumLapTimeReset(Id));
    }
    public void SetMaximumLapTime(TimeSpan maximumLapTime)
    {
        if (maximumLapTime <= TimeSpan.Zero)
        {
            throw new ValidationException("Maximum lap time must be greater than zero.");
        }
        if (maximumLapTime == _maximumLapTime)
        {
            return;
        }
        Raise(new OpenPracticeSessionMaximumLapTimeSet(Id, maximumLapTime));
    }

    public void Clone(Guid newId, string newName, Guid? trackOverride)
    {
        Raise(new OpenPracticeSessionCloned(Id, newId, newName, trackOverride));
    }

    public void ChangeTrack(Guid trackId)
    {
        if(_trackId == trackId)
        {
            return;
        }

        Raise(new OpenPracticeSessionTrackChanged(Id, _trackId, trackId));
    }
    private void When(OpenPracticeSessionTrackChanged changed)
    {
        _trackId = changed.NewTrackId;
    }
}
