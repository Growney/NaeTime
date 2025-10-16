using EventDbLite.Aggregates;
using NaeTime.Events;
namespace NaeTime.Command.Aggregates;
public class OpenPracticeSession : AggregateRoot<Guid>
{
    private class LaneInfo(Guid? pilotId, bool isEnabled, OpenPracticeSession.LaneFrequency frequency)
    {
        public Guid? PilotId { get; set; } = pilotId;
        public bool IsEnabled { get; set; } = isEnabled;
        public LaneFrequency Frequency { get; set; } = frequency ?? throw new ArgumentNullException(nameof(frequency));
    }
    private class LaneFrequency(byte? bandId, int frequencyInMHz)
    {
        public byte? BandId { get; set; } = bandId;
        public int FrequencyInMHz { get; set; } = frequencyInMHz;
    }
    private readonly Dictionary<byte, LaneInfo> _lanes = [];
    public Guid TrackId { get; private set; }
    public OpenPracticeSession()
    {

    }
    public OpenPracticeSession(Guid id, Guid trackId, string name)
    {
        Raise(new OpenPracticeSessionScheduled(id, name, trackId));
    }

    public void Rename(string name)
    {
        Raise(new OpenPracticeSessionRenamed(Id, name));
    }

    public void ConfigureDefaultLanes(byte laneCount)
    {
        for (byte i = 0; i < laneCount; i++)
        {
            Raise(new OpenPracticeSessionLaneEnabled(Id, i));

            int frequencyIndex = i % Hardware.Frequency.Band.R.Frequencies.Count();

            Raise(new OpenPracticeSessionLaneVideoFrequencyTuned(Id, i, Hardware.Frequency.Band.R.Id, Hardware.Frequency.Band.R.Frequencies.ElementAt(frequencyIndex).FrequencyInMhz));
        }
    }

    private LaneInfo GetLaneInfo(byte lane)
    {
        if (!_lanes.TryGetValue(lane, out LaneInfo? laneInfo))
        {
            laneInfo = new LaneInfo(null, false, new LaneFrequency(null, 0));
            _lanes.Add(lane, laneInfo);
        }
        return laneInfo;
    }

    private void When(OpenPracticeSessionScheduled scheduledEvent)
    {
        Id = scheduledEvent.SessionId;
        TrackId = scheduledEvent.TrackId;
    }
    private void When(OpenPracticeSessionLaneEnabled laneEnabled)
    {
        LaneInfo laneInfo = GetLaneInfo(laneEnabled.Lane);
        laneInfo.IsEnabled = true;
    }
    private void When(OpenPracticeSessionLaneDisabled laneDisabled)
    {
        LaneInfo laneInfo = GetLaneInfo(laneDisabled.Lane);
        laneInfo.IsEnabled = false;
    }
    private void When(OpenPracticeSessionLaneVideoFrequencyTuned laneVideoFrequencyTuned)
    {
        LaneInfo laneInfo = GetLaneInfo(laneVideoFrequencyTuned.Lane);
        laneInfo.Frequency.BandId = laneVideoFrequencyTuned.BandId;
        laneInfo.Frequency.FrequencyInMHz = laneVideoFrequencyTuned.FrequencyInMHz;
    }
    private void When(OpenPracticeSessionLanePilotSet lanePilotSet)
    {
        LaneInfo laneInfo = GetLaneInfo(lanePilotSet.Lane);
        laneInfo.PilotId = lanePilotSet.PilotId;
    }
    private void When(OpenPracticeSessionLanePilotReset lanePilotReset)
    {
        LaneInfo laneInfo = GetLaneInfo(lanePilotReset.Lane);
        laneInfo.PilotId = null;
    }

    public void SetLanePilot(byte lane, Guid pilotId)
    {
        foreach (LaneInfo laneInfo in _lanes.Values)
        {
            if (laneInfo.PilotId == pilotId)
            {
                throw new InvalidOperationException($"Pilot {pilotId} is already assigned to a lane.");
            }
        }

        Raise(new OpenPracticeSessionLanePilotSet(Id, lane, pilotId));
    }
    public void ResetLanePilot(byte lane)
    {
        if (_lanes.TryGetValue(lane, out LaneInfo? laneInfo))
        {
            Raise(new OpenPracticeSessionLanePilotReset(Id, lane));
        }
    }
    public void TuneLaneVideoFrequency(byte lane, byte? bandId, int frequencyInMHz)
    {
        Raise(new OpenPracticeSessionLaneVideoFrequencyTuned(Id, lane, bandId, frequencyInMHz));
    }
    public void EnableLane(byte lane)
    {
        if (_lanes.TryGetValue(lane, out LaneInfo? laneInfo))
        {
            if (!laneInfo.IsEnabled)
            {
                Raise(new OpenPracticeSessionLaneEnabled(Id, lane));
            }
        }
        else
        {
            Raise(new OpenPracticeSessionLaneEnabled(Id, lane));
        }
    }
    public void DisableLane(byte lane)
    {
        if (_lanes.TryGetValue(lane, out LaneInfo? laneInfo))
        {
            if (laneInfo.IsEnabled)
            {
                Raise(new OpenPracticeSessionLaneDisabled(Id, lane));
            }
        }
        else
        {
            Raise(new OpenPracticeSessionLaneDisabled(Id, lane));
        }
    }
    public OpenPracticeSession Clone(Guid newId, string newName)
    {
        OpenPracticeSession clone = new(newId, TrackId, newName);
        Clone(clone);
        CloneLanes(clone);

        return clone;
    }
    public OpenPracticeSession Clone(Guid newId, Guid newTrackId, string newName)
    {
        OpenPracticeSession clone = new(newId, newTrackId, newName);
        Clone(clone);
        CloneLanes(clone);

        return clone;
    }
    public void AssignHardwareDetection(Guid detectionId, Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        Raise(new OpenPracticeHardwareDetectionOccured(detectionId, Id, timerId, TrackId, lane, hardwareTime, softwareTime, utcTime));

        LaneInfo laneInfo = GetLaneInfo(lane);
        if (!laneInfo.IsEnabled)
        {
            Raise(new OpenPracticeHardwareDetectionIgnoredOnDisabledLane(detectionId, Id, timerId, TrackId, lane, hardwareTime, softwareTime, utcTime));
            return;
        }
        if (laneInfo.PilotId is null)
        {
            Raise(new OpenPracticeHardwareDetectionIgnoredOnUnassignedLane(detectionId, Id, timerId, TrackId, lane, hardwareTime, softwareTime, utcTime));
            return;
        }

        Raise(new OpenPracticeHardwareDetectionAssignedToPilot(detectionId, Id, laneInfo.PilotId.Value, timerId, TrackId, lane, hardwareTime, softwareTime, utcTime));
    }

    private void CloneLanes(OpenPracticeSession clone)
    {
        foreach (KeyValuePair<byte, LaneInfo> laneInfo in _lanes)
        {
            if (laneInfo.Value.IsEnabled)
            {
                clone.EnableLane(laneInfo.Key);
            }
            else
            {
                clone.DisableLane(laneInfo.Key);
            }
            if (laneInfo.Value.PilotId.HasValue)
            {
                clone.SetLanePilot(laneInfo.Key, laneInfo.Value.PilotId.Value);
            }
            clone.TuneLaneVideoFrequency(laneInfo.Key, laneInfo.Value.Frequency.BandId, laneInfo.Value.Frequency.FrequencyInMHz);
        }
    }
}
