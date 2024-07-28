using NaeTime.Announcer.Abstractions;
using NaeTime.Announcer.Models;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
namespace NaeTime.Announcer;
public class OpenPracticeLapAnnouncer : IAnnouncmentProvider
{
    private readonly long _minimumAnnouncmentInterval = 1000;
    private readonly long _maximumAnnouncmentInterval = 5000;
    private readonly long _maxEventInterval = 500;

    private readonly Stopwatch _stopwatch = new();
    private readonly ConcurrentDictionary<Guid, SessionState> _sessionStates = new();
    private readonly IRemoteProcedureCallClient _rpcClient;

    private class ConsecutiveLapRecord
    {
        public Guid PilotId { get; init; }
        public uint LapCap { get; init; }
        public uint Laps { get; init; }
        public long TotalMilliseconds { get; init; }
        public IEnumerable<Guid> IncludedLaps { get; init; } = Enumerable.Empty<Guid>();
    }
    private class SingleLapTime
    {
        public Guid PilotId { get; init; }
        public long TotalMilliseconds { get; init; }
    }

    private class SessionState
    {
        public long? StateStart { get; set; }
        public long? LastUpdate { get; set; }
        public ConcurrentDictionary<uint, ConsecutiveLapRecord> ConsecutiveLapRecordHolder { get; } = new();
        public SingleLapTime? SingleLapRecordHolder { get; set; }


        public ConcurrentQueue<SingleLapTime> PilotLaps { get; } = new();
        public ConcurrentDictionary<Guid, SingleLapTime> PilotLapRecord { get; } = new();
        public ConcurrentDictionary<Guid, ConcurrentDictionary<uint, ConsecutiveLapRecord>> PilotConsecutiveLapRecords { get; } = new();

        public void TickState(long elapsed)
        {
            StateStart ??= elapsed;
            LastUpdate = elapsed;
        }
        public void ClearState()
        {
            StateStart = null;
            ConsecutiveLapRecordHolder.Clear();
            SingleLapRecordHolder = null;

            PilotLaps.Clear();
            PilotLapRecord.Clear();
            PilotConsecutiveLapRecords.Clear();
        }
        public bool IsEmpty() => PilotLaps.Count == 0 && PilotLapRecord.Count == 0 && PilotConsecutiveLapRecords.Count == 0 && ConsecutiveLapRecordHolder.Count == 0 && SingleLapRecordHolder == null;
        public bool HasAnyRecords() => ConsecutiveLapRecordHolder.Count > 0 || SingleLapRecordHolder != null || PilotLapRecord.Count > 0 || PilotConsecutiveLapRecords.Count > 0;
        public bool HasAnySessionRecords() => ConsecutiveLapRecordHolder.Count > 0 || SingleLapRecordHolder != null;
        //Don't need to check the session records because they will always be included in the pilot records
        public bool HasMultiPilotRecords() => PilotLapRecord.Count > 1 || PilotConsecutiveLapRecords.Count > 1;
        public bool IsMultiPilot()
        {
            if (PilotLapRecord.Count > 1)
            {
                return true;
            }

            if (PilotConsecutiveLapRecords.Count > 1)
            {
                return true;
            }

            if (PilotLaps.Count > 1)
            {
                Guid? firstPilot = PilotLaps.FirstOrDefault()?.PilotId;

                return PilotLaps.Skip(1).Any(lap => lap.PilotId != firstPilot);
            }

            if (ConsecutiveLapRecordHolder.Count > 1)
            {
                Guid? firstPilot = ConsecutiveLapRecordHolder.Values.FirstOrDefault()?.PilotId;

                return ConsecutiveLapRecordHolder.Values.Skip(1).Any(record => record.PilotId != firstPilot);
            }

            return false;
        }
        public Guid GetSinglePilotId()
        {
            if (IsEmpty())
            {
                throw new Exception("Session state is empty");
            }

            if (IsMultiPilot())
            {
                throw new Exception("Cannot get single pilot id when there are multiple pilots");
            }

            if (PilotLapRecord.Count > 0)
            {
                return PilotLapRecord.Keys.First();
            }

            if (PilotConsecutiveLapRecords.Count > 1)
            {
                return PilotLapRecord.Keys.First();
            }

            if (SingleLapRecordHolder != null)
            {
                return SingleLapRecordHolder.PilotId;
            }

            if (ConsecutiveLapRecordHolder.Count > 0)
            {
                return ConsecutiveLapRecordHolder.Values.First().PilotId;
            }

            return PilotLaps.First().PilotId;
        }
        public Guid GetOnlyPilotWithRecords()
        {
            if (HasMultiPilotRecords())
            {
                throw new Exception("Cannot get only pilot with records when there are multiple pilots with records");
            }

            if (PilotLapRecord.Count > 0)
            {
                return PilotLapRecord.Keys.First();
            }

            return PilotConsecutiveLapRecords.Keys.First();
        }
        public IEnumerable<Guid> GetPilotIds()
        {
            List<Guid> pilotIds = new();

            foreach (ConsecutiveLapRecord record in ConsecutiveLapRecordHolder.Values)
            {
                if (pilotIds.Contains(record.PilotId))
                {
                    continue;
                }

                pilotIds.Add(record.PilotId);
            }

            if (SingleLapRecordHolder != null)
            {
                if (pilotIds.Contains(SingleLapRecordHolder.PilotId))
                {
                    return pilotIds;
                }

                pilotIds.Add(SingleLapRecordHolder.PilotId);
            }

            foreach (Guid pilotId in PilotConsecutiveLapRecords.Keys)
            {
                if (pilotIds.Contains(pilotId))
                {
                    continue;
                }

                pilotIds.Add(pilotId);
            }

            foreach (Guid pilotId in PilotLapRecord.Keys)
            {
                if (pilotIds.Contains(pilotId))
                {
                    continue;
                }

                pilotIds.Add(pilotId);
            }

            foreach (SingleLapTime lap in PilotLaps)
            {
                if (pilotIds.Contains(lap.PilotId))
                {
                    continue;
                }

                pilotIds.Add(lap.PilotId);
            }


            return pilotIds;
        }
    }
    public OpenPracticeLapAnnouncer(IRemoteProcedureCallClient rpcClient)
    {
        _stopwatch.Start();
        _rpcClient = rpcClient;
    }

    public async Task<Announcement?> GetNextAnnouncement()
    {
        foreach (SessionState state in _sessionStates.Values)
        {
            Announcement? announcement = await GenerateStateAnnouncement(state);

            if (announcement != null)
            {
                return announcement;
            }
        }

        return null;
    }

    private async Task<Announcement?> GenerateStateAnnouncement(SessionState state)
    {
        if (state.IsEmpty())
        {
            return null;
        }

        long elapsed = _stopwatch.ElapsedMilliseconds;
        //Session state has not started yet
        if (state.StateStart == null)
        {
            return null;
        }

        if (state.LastUpdate == null)
        {
            return null;
        }

        long timeSinceStart = elapsed - state.StateStart.Value;
        //Too early to announce the session state
        if (timeSinceStart < _minimumAnnouncmentInterval)
        {
            return null;
        }

        long timeSinceLastUpdate = elapsed - state.LastUpdate.Value;
        //To soon since the last event to announce the session and we have not reached max time yet
        if (timeSinceLastUpdate < _maxEventInterval && timeSinceStart < _maximumAnnouncmentInterval)
        {
            return null;
        }

        string message = await GenerateAnnouncementText(state);
        //If we clear the state and have no message something went wrong but for now we will just ignore that it happened and stay silent
        state.ClearState();

        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        return new Announcement(message);
    }

    private async Task<string> GenerateAnnouncementText(SessionState state)
    {
        StringBuilder announcementText = new();

        IEnumerable<Guid> pilotIds = state.GetPilotIds();

        foreach (Guid pilotId in pilotIds)
        {
            string? pilotAnnouncement = await GeneratePilotRecordAnnouncement(state, pilotId);

            if (string.IsNullOrWhiteSpace(pilotAnnouncement))
            {
                continue;
            }

            if (announcementText.Length > 0)
            {
                announcementText.Append(", ");
            }

            announcementText.Append(pilotAnnouncement);
        }

        string message = announcementText.ToString();
        return message;
    }
    private async Task<string?> GeneratePilotRecordAnnouncement(SessionState state, Guid pilotId)
    {
        string? callout = await GetPilotCallout(pilotId);

        if (string.IsNullOrWhiteSpace(callout))
        {
            return null;
        }

        uint? lapCap = null;
        if (state.SingleLapRecordHolder?.PilotId == pilotId)
        {
            lapCap = 1;
        }

        if (state.ConsecutiveLapRecordHolder.Values.Any(x => x.PilotId == pilotId))
        {
            lapCap = state.ConsecutiveLapRecordHolder.Values.Where(x => x.PilotId == pilotId).Max(x => x.LapCap);
        }

        if (lapCap.HasValue)
        {
            if (lapCap == 1)
            {
                SingleLapTime? record = state.SingleLapRecordHolder;
                if (record != null)
                {
                    return $"Single Lap Record, {callout}, {GetLapCallout(record!.TotalMilliseconds)}";
                }
            }
            else
            {
                if (state.ConsecutiveLapRecordHolder.TryGetValue(lapCap.Value, out ConsecutiveLapRecord? record))
                {
                    return $"{record.LapCap} Lap Record, {callout} with {await GetLapTimesAnnouncement(record.IncludedLaps)}, totalling {GetLapCallout(record.TotalMilliseconds)}";
                }
            }
        }

        if (state.PilotLapRecord.ContainsKey(pilotId))
        {
            lapCap = 1;
        }

        if (state.PilotConsecutiveLapRecords.ContainsKey(pilotId))
        {
            lapCap = state.PilotConsecutiveLapRecords[pilotId].Values.Max(x => x.LapCap);
        }

        if (lapCap.HasValue)
        {
            if (lapCap == 1)
            {
                if (state.PilotLapRecord.TryGetValue(pilotId, out SingleLapTime? record))
                {
                    return $"Single Lap PB, {callout}, {GetLapCallout(record.TotalMilliseconds)}";
                }

            }
            else
            {
                if (state.PilotConsecutiveLapRecords.TryGetValue(pilotId, out ConcurrentDictionary<uint, ConsecutiveLapRecord>? records)
                    &&
                    records.TryGetValue(lapCap.Value, out ConsecutiveLapRecord? record))
                {
                    return $"{record.LapCap} Lap PB, {callout}, {GetLapCallout(record.TotalMilliseconds)}";
                }
            }
        }

        if (!state.PilotLaps.Any(x => x.PilotId == pilotId))
        {
            return null;
        }

        return $"{callout}, {GetLapCallout(state.PilotLaps.Where(x => pilotId == x.PilotId))}";
    }
    private async Task<string> GetLapTimesAnnouncement(IEnumerable<Guid> lapIds)
    {
        IEnumerable<OpenPractice.Messages.Models.Lap>? laps = await _rpcClient.InvokeAsync<IEnumerable<OpenPractice.Messages.Models.Lap>>("GetOpenPracticeLaps", lapIds);

        if (laps == null)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        bool firstLap = true;
        foreach (OpenPractice.Messages.Models.Lap lap in laps)
        {
            if (!firstLap)
            {
                builder.Append(", ");
            }

            builder.Append(GetLapCallout(lap.TotalMilliseconds, 1));
            firstLap = false;
        }

        return builder.ToString();
    }
    private async Task<string?> GetPilotCallout(Guid pilotId)
    {
        Management.Messages.Models.Pilot? pilot = await _rpcClient.InvokeAsync<Management.Messages.Models.Pilot>("GetPilot", pilotId);

        if (pilot == null)
        {
            return null;
        }

        return GetPilotCallout(pilot);
    }
    private string? GetPilotCallout(Management.Messages.Models.Pilot pilot) => pilot.CallSign ?? pilot.FirstName ?? pilot.LastName;
    private string GetLapCallout(long totalMilliseconds, int roundedTo = 3)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(totalMilliseconds);
        return Math.Round(timeSpan.TotalSeconds, roundedTo).ToString();
    }
    private string GetLapCallout(IEnumerable<SingleLapTime> laps, int roundedTo = 1)
    {
        StringBuilder builder = new();
        bool isFirstLap = true;
        foreach (SingleLapTime lap in laps)
        {
            if (!isFirstLap)
            {
                builder.Append(", ");
            }

            builder.Append(GetLapCallout(lap.TotalMilliseconds, roundedTo));

            isFirstLap = false;
        }
        return builder.ToString();
    }

    public void When(ConsecutiveLapLeaderboardPositionImproved improved)
    {
        SessionState state = _sessionStates.GetOrAdd(improved.SessionId, _ => new SessionState());

        ConsecutiveLapRecord record = new()
        {
            PilotId = improved.PilotId,
            LapCap = improved.LapCap,
            Laps = improved.TotalLaps,
            TotalMilliseconds = improved.TotalMilliseconds,
            IncludedLaps = improved.IncludedLaps
        };

        //Ignore it when they have not yet reached the consecutive lap cap
        if (improved.LapCap != improved.TotalLaps)
        {
            return;
        }

        if (improved.NewPosition == 0)
        {
            state.ConsecutiveLapRecordHolder.AddOrUpdate(improved.LapCap, record, (key, toUpdate) => record);
        }

        state.PilotConsecutiveLapRecords.AddOrUpdate(improved.PilotId,
        (pilotId) =>
        {
            ConcurrentDictionary<uint, ConsecutiveLapRecord> newDictionary = new();
            newDictionary.AddOrUpdate(record.LapCap, record, (key, toUpdate) => record);
            return newDictionary;
        },
        (key, toUpdate) =>
        {
            toUpdate.AddOrUpdate(record.LapCap, record, (key, toUpdate) => record);
            return toUpdate;
        });


        state.TickState(_stopwatch.ElapsedMilliseconds);
    }
    public void When(ConsecutiveLapLeaderboardRecordImproved improved)
    {
        SessionState state = _sessionStates.GetOrAdd(improved.SessionId, _ => new SessionState());

        ConsecutiveLapRecord record = new()
        {
            PilotId = improved.PilotId,
            LapCap = improved.LapCap,
            Laps = improved.TotalLaps,
            TotalMilliseconds = improved.TotalMilliseconds,
            IncludedLaps = improved.IncludedLaps
        };

        //Ignore it when they have not yet reached the consecutive lap cap
        if (improved.LapCap != improved.TotalLaps)
        {
            return;
        }

        if (improved.Position == 0)
        {
            state.ConsecutiveLapRecordHolder.AddOrUpdate(improved.LapCap, record, (key, toUpdate) => record);
        }

        state.PilotConsecutiveLapRecords.AddOrUpdate(improved.PilotId,
        (pilotId) =>
        {
            ConcurrentDictionary<uint, ConsecutiveLapRecord> newDictionary = new();
            newDictionary.AddOrUpdate(record.LapCap, record, (key, toUpdate) => record);
            return newDictionary;
        },
        (key, toUpdate) =>
        {
            toUpdate.AddOrUpdate(record.LapCap, record, (key, toUpdate) => record);
            return toUpdate;
        });

        state.TickState(_stopwatch.ElapsedMilliseconds);
    }
    public void When(SingleLapLeaderboardPositionImproved improved)
    {
        SessionState state = _sessionStates.GetOrAdd(improved.SessionId, _ => new SessionState());

        SingleLapTime record = new()
        {
            PilotId = improved.PilotId,
            TotalMilliseconds = improved.TotalMilliseconds
        };

        if (improved.NewPosition == 0)
        {
            state.SingleLapRecordHolder = record;
        }

        state.PilotLapRecord.AddOrUpdate(improved.PilotId, record, (key, toUpdate) => record);

        state.TickState(_stopwatch.ElapsedMilliseconds);
    }
    public void When(SingleLapLeaderboardRecordImproved improved)
    {
        SessionState state = _sessionStates.GetOrAdd(improved.SessionId, _ => new SessionState());

        SingleLapTime record = new()
        {
            PilotId = improved.PilotId,
            TotalMilliseconds = improved.TotalMilliseconds
        };

        if (improved.Position == 0)
        {
            state.SingleLapRecordHolder = record;
        }

        state.PilotLapRecord.AddOrUpdate(improved.PilotId, record, (key, toUpdate) => record);

        state.TickState(_stopwatch.ElapsedMilliseconds);
    }
    public void When(OpenPracticeLapCompleted lapCompleted)
    {
        SessionState state = _sessionStates.GetOrAdd(lapCompleted.SessionId, _ => new SessionState());

        SingleLapTime record = new()
        {
            PilotId = lapCompleted.PilotId,
            TotalMilliseconds = lapCompleted.TotalMilliseconds
        };

        state.PilotLaps.Enqueue(record);

        state.TickState(_stopwatch.ElapsedMilliseconds);
    }
}