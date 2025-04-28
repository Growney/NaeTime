using Microsoft.EntityFrameworkCore;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Timing;
using NaeTime.Persistence.EntityFramework;

namespace NaeTime.Orchestrator.Persistence.EntityFramework;

public class TimingOrchestratorPersistence : ITimingOrchestratorPersistence
{
    private readonly NaeTimeDbContext _dbContext;

    public TimingOrchestratorPersistence(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Guid> AddDetection(Guid sessionId, Guid trackId, Guid timerId, int timerIndex, byte lane, Guid? pilotId, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        Guid detectionId = Guid.NewGuid();

        _dbContext.OpenPracticeDetections.Add(new NaeTime.Persistence.EntityFramework.Models.OpenPracticeDetection()
        {
            SessionId = sessionId,
            TrackId = trackId,
            TimerId = timerId,
            TimerIndex = timerIndex,
            Lane = lane,
            PilotId = pilotId,
            HardwareTime = hardwareTime,
            SoftwareTime = softwareTime,
            UtcTime = utcTime
        });

        return Task.FromResult(detectionId);
    }
    public async Task<IEnumerable<Detection>> GetSessionPilotDetections(Guid sessionId, Guid pilotId)
        => await _dbContext.OpenPracticeDetections.Where(x => x.SessionId == sessionId && x.PilotId == pilotId)
        .Select(x => new Detection(x.Id, x.SessionId, x.TrackId, x.TimerId, x.TimerIndex, x.Lane, x.PilotId, x.HardwareTime, x.SoftwareTime, x.UtcTime))
        .ToListAsync();

    public Task<Guid> AddLap(Guid sessionId, Guid pilotId, Guid entryDetection, LapStatus status) => AddLap(sessionId, pilotId, entryDetection, null, status);
    public Task<Guid> AddLap(Guid sessionId, Guid pilotId, Guid entryDetection, Guid? exitDetection, LapStatus status)
    {
        Guid lapId = Guid.NewGuid();
        _dbContext.OpenPracticeLaps.Add(new NaeTime.Persistence.EntityFramework.Models.OpenPracticeLap()
        {
            Id = lapId,
            SessionId = sessionId,
            PilotId = pilotId,
            EntryDetectionId = entryDetection,
            ExitDetectionId = exitDetection,
            Status = status switch
            {
                LapStatus.Invalid => NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Invalid,
                LapStatus.Valid => NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Valid,
                LapStatus.Incomplete => NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Incomplete,
                _ => throw new NotImplementedException()
            }
        });

        return Task.FromResult(lapId);
    }
    public Task CompleteLap(Guid lapId, Guid exitDetection, LapStatus status)
    {
        NaeTime.Persistence.EntityFramework.Models.OpenPracticeLap? lap = _dbContext.OpenPracticeLaps.FirstOrDefault(x => x.Id == lapId);
        if (lap == null)
        {
            throw new KeyNotFoundException("Lap not found");
        }
        lap.ExitDetectionId = exitDetection;
        lap.Status = status switch
        {
            LapStatus.Invalid => NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Invalid,
            LapStatus.Valid => NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Valid,
            LapStatus.Incomplete => NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Incomplete,
            _ => throw new NotImplementedException()
        };
        return Task.CompletedTask;
    }
    public Task UpdateLap(Guid lapId, Guid entryDetection, Guid? exitDetection, LapStatus status)
    {
        NaeTime.Persistence.EntityFramework.Models.OpenPracticeLap? lap = _dbContext.OpenPracticeLaps.FirstOrDefault(x => x.Id == lapId);
        if (lap == null)
        {
            throw new KeyNotFoundException("Lap not found");
        }
        lap.EntryDetectionId = entryDetection;
        lap.ExitDetectionId = exitDetection;
        lap.Status = status switch
        {
            LapStatus.Invalid => NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Invalid,
            LapStatus.Valid => NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Valid,
            LapStatus.Incomplete => NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Incomplete,
            _ => throw new NotImplementedException()
        };
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Lap>> GetOpenPracticeLaps(IEnumerable<Guid> lapIds)
    {
        var laps = await _dbContext.OpenPracticeLaps.Where(x => lapIds.Contains(x.Id)).ToListAsync();
        var detectionIds = laps.SelectMany(x =>
        {
            if (x.ExitDetectionId.HasValue)
            {
                return new[] { x.EntryDetectionId, x.ExitDetectionId.Value };
            }
            else
            {
                return new[] { x.EntryDetectionId };
            }
        });
        var detections = await _dbContext.OpenPracticeDetections.Where(x => detectionIds.Contains(x.Id))
            .Select(x => new Detection(x.Id, x.SessionId, x.TrackId, x.TimerId, x.TimerIndex, x.Lane, x.PilotId, x.HardwareTime, x.SoftwareTime, x.UtcTime))
            .ToDictionaryAsync(x => x.Id);

        return GetLaps(laps, detections);
    }

    public async Task<IEnumerable<Lap>> GetPilotOpenPracticeSessionLaps(Guid sessionId, Guid pilotId)
    {
        var laps = await _dbContext.OpenPracticeLaps.Where(x => x.SessionId == sessionId && x.PilotId == pilotId).ToListAsync();
        var detections = await _dbContext.OpenPracticeDetections.Where(x => x.SessionId == sessionId && x.PilotId == pilotId)
            .Select(x => new Detection(x.Id, x.SessionId, x.TrackId, x.TimerId, x.TimerIndex, x.Lane, x.PilotId, x.HardwareTime, x.SoftwareTime, x.UtcTime))
            .ToDictionaryAsync(x => x.Id);

        return GetLaps(laps, detections);
    }
    private IEnumerable<Lap> GetLaps(List<NaeTime.Persistence.EntityFramework.Models.OpenPracticeLap> laps, Dictionary<Guid, Detection> detections) =>
       laps.Select(x =>
       {
           if (!detections.TryGetValue(x.EntryDetectionId, out var entryDetection))
           {
               throw new KeyNotFoundException("Entry detection not found");
           }
           Detection? exitDetection = null;
           if (x.ExitDetectionId.HasValue)
           {
               detections.TryGetValue(x.ExitDetectionId.Value, out exitDetection);
           }
           return new Lap(x.Id,
               x.SessionId,
               x.PilotId,
               entryDetection,
               exitDetection,
               x.Status switch
               {
                   NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Invalid => LapStatus.Invalid,
                   NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Valid => LapStatus.Valid,
                   NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Incomplete => LapStatus.Incomplete,
                   _ => throw new NotImplementedException()
               });
       });
    public async Task<IEnumerable<Lap>> GetOpenPracticeSessionLaps(Guid sessionId)
    {
        var laps = await _dbContext.OpenPracticeLaps.Where(x => x.SessionId == sessionId).ToListAsync();
        var detections = await _dbContext.OpenPracticeDetections.Where(x => x.SessionId == sessionId)
            .Select(x => new Detection(x.Id, x.SessionId, x.TrackId, x.TimerId, x.TimerIndex, x.Lane, x.PilotId, x.HardwareTime, x.SoftwareTime, x.UtcTime))
            .ToDictionaryAsync(x => x.Id);

        return GetLaps(laps, detections);
    }

    public async Task<Lap?> GetOpenPracticeSessionLap(Guid lapId)
    {
        var lap = await _dbContext.OpenPracticeLaps.Where(x => x.Id == lapId).FirstOrDefaultAsync();

        if (lap == null)
        {
            return null;
        }

        var detections = await _dbContext.OpenPracticeDetections.Where(x => x.Id == lap.EntryDetectionId || x.Id == lap.ExitDetectionId)
            .Select(x => new Detection(x.Id, x.SessionId, x.TrackId, x.TimerId, x.TimerIndex, x.Lane, x.PilotId, x.HardwareTime, x.SoftwareTime, x.UtcTime))
            .ToListAsync();

        return new Lap(lap.Id,
            lap.SessionId,
            lap.PilotId,
            detections.FirstOrDefault(x => x.Id == lap.EntryDetectionId) ?? throw new KeyNotFoundException("Entry detection not found"),
            detections.FirstOrDefault(x => x.Id == lap.ExitDetectionId),
            lap.Status switch
            {
                NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Invalid => LapStatus.Invalid,
                NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Valid => LapStatus.Valid,
                NaeTime.Persistence.EntityFramework.Models.OpenPracticeLapStatus.Incomplete => LapStatus.Incomplete,
                _ => throw new NotImplementedException()
            });
    }
}
