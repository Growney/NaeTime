using Microsoft.EntityFrameworkCore;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.OpenPractice;
using NaeTime.Persistence.EntityFramework;

namespace NaeTime.Orchestrator.Persistence.EntityFramework;

public class OpenPracticeOrchestratorPersistence : IOpenPracticeOrchestratorPersistence
{
    private readonly NaeTimeDbContext _dbContext;

    public OpenPracticeOrchestratorPersistence(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ConfigureLanePilot(Guid sessionId, byte lane, Guid pilotId)
    {
        NaeTime.Persistence.EntityFramework.Models.OpenPracticeLaneConfiguration? laneConfiguration = await _dbContext.OpenPracticeLaneConfigurations.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.OpenPracticeLaneConfiguration
            {
                SessionId = sessionId,
                Lane = lane,
                PilotId = pilotId
            };
            _dbContext.OpenPracticeLaneConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.PilotId = pilotId;
        }
    }
    public async Task ConfigureLaneRadioFrequency(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz)
    {
        NaeTime.Persistence.EntityFramework.Models.OpenPracticeLaneConfiguration? laneConfiguration = await _dbContext.OpenPracticeLaneConfigurations.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.OpenPracticeLaneConfiguration
            {
                SessionId = sessionId,
                Lane = lane,
                BandId = bandId,
                FrequencyInMhz = frequencyInMhz
            };
            _dbContext.OpenPracticeLaneConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.BandId = bandId;
            laneConfiguration.FrequencyInMhz = frequencyInMhz;
        }
    }
    public async Task ConfigureLaneStatus(Guid sessionId, byte lane, bool isEnabled)
    {
        NaeTime.Persistence.EntityFramework.Models.OpenPracticeLaneConfiguration? laneConfiguration = await _dbContext.OpenPracticeLaneConfigurations.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.OpenPracticeLaneConfiguration
            {
                SessionId = sessionId,
                Lane = lane,
                IsEnabled = isEnabled
            };
            _dbContext.OpenPracticeLaneConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.IsEnabled = isEnabled;
        }
    }
    public async Task<IEnumerable<PilotLaneConfiguration>> GetLaneConfigurations(Guid sessionId) => await _dbContext.OpenPracticeLaneConfigurations.Where(x => x.SessionId == sessionId).Select(x => new PilotLaneConfiguration(x.Lane, x.PilotId)).ToListAsync();
    public async Task<OpenPracticeSession?> GetOpenPracticeSession(Guid sessionId)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId).ConfigureAwait(false);

        if (session == null)
        {
            return null;
        }

        return new OpenPracticeSession(session.Id, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, session.TrackedConsecutiveLaps.Select(x => x.LapCap));
    }
    public async Task<IEnumerable<AverageLapLeaderboardPosition>> GetOpenPracticeSessionAverageLapLeaderboardPositions(Guid sessionId)
    {
        var positions = await _dbContext.AverageLapLeaderboardPositions.Where(x => x.SessionId == sessionId).ToListAsync();

        return positions.Select(x => new AverageLapLeaderboardPosition(x.Position, x.PilotId, x.AverageMilliseconds, x.FirstLapCompletionUtc));
    }
    public async Task<IEnumerable<ConsecutiveLapLeaderboardPosition>> GetOpenPracticeSessionConsecutiveLapsLeaderboardPositions(Guid sessionId, uint lapCap)
    {
        var positions = await _dbContext.ConsecutiveLapLeaderboardPositions.Where(x => x.SessionId == sessionId && x.LapCap == lapCap).ToListAsync();

        return positions.Select(x => new ConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletionUtc, x.IncludedLaps.Select(x => x.LapId)));
    }

    public async Task<IEnumerable<LapRecord>> GetOpenPracticeSessionLapPilotLapRecords(Guid sessionId, Guid pilotId)
    {
        List<LapRecord> records = new();
        var singleLapRecord = await _dbContext.SingleLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId).ConfigureAwait(false);

        if (singleLapRecord != null)
        {
            records.Add(new LapRecord(1, [singleLapRecord.LapId]));
        }

        var consecutiveLapRecords = await _dbContext.ConsecutiveLapLeaderboardPositions.Where(x => x.SessionId == sessionId && x.PilotId == pilotId).ToListAsync().ConfigureAwait(false);

        if (consecutiveLapRecords.Any())
        {
            records.AddRange(consecutiveLapRecords.Select(x => new LapRecord(x.LapCap, x.IncludedLaps.Select(x => x.LapId))));
        }

        return records;
    }

    public async Task<IEnumerable<OpenPracticeSession>> GetOpenPracticeSessions()
    {
        var sessions = await _dbContext.OpenPracticeSessions.ToListAsync().ConfigureAwait(false);

        List<OpenPracticeSession> responseSessions = new();
        foreach (var session in sessions)
        {
            responseSessions.Add(new OpenPracticeSession(session.Id, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, session.TrackedConsecutiveLaps.Select(y => y.LapCap)));
        }

        return responseSessions;
    }
    public async Task<IEnumerable<SingleLapLeaderboardPosition>> GetOpenPracticeSessionSingleLapLeaderboardPositions(Guid sessionId)
    {
        var positions = await _dbContext.SingleLapLeaderboardPositions.Where(x => x.SessionId == sessionId).ToListAsync();

        return positions.Select(x => new SingleLapLeaderboardPosition(x.Position, x.PilotId, x.TotalMilliseconds, x.CompletionUtc, x.LapId));
    }
    public async Task<IEnumerable<TotalLapLeaderboardPosition>> GetOpenPracticeSessionTotalLapLeaderboardPositions(Guid sessionId)
    {
        var positions = await _dbContext.TotalLapsLeaderboardPositions.Where(x => x.SessionId == sessionId).ToListAsync();

        return positions.Select(x => new TotalLapLeaderboardPosition(x.Position, x.PilotId, x.TotalLaps, x.FirstLapCompletionUtc));
    }
    public async Task<AverageLapRecord?> GetPilotOpenPracticeSessionAverageLapRecord(Guid sessionId, Guid pilotId)
    {
        var position = await _dbContext.AverageLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        return position == null
            ? null
            : new AverageLapRecord(position.AverageMilliseconds, position.FirstLapCompletionUtc);
    }

    public async Task<IEnumerable<ConsecutiveLapRecord>> GetPilotOpenPracticeSessionConsecutiveLapRecords(Guid sessionId, Guid pilotId)
    {
        var positions = await _dbContext.ConsecutiveLapLeaderboardPositions.Where(x => x.SessionId == sessionId && x.PilotId == pilotId).ToListAsync();

        return positions.Select(x => new ConsecutiveLapRecord(x.LapCap, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletionUtc, x.IncludedLaps.Select(x => x.LapId)));
    }

    public async Task<IEnumerable<uint>> GetOpenPracticeSessionTrackedConsecutiveLaps(Guid sessionId)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId).ConfigureAwait(false);

        return session == null ? Enumerable.Empty<uint>() : session.TrackedConsecutiveLaps.Select(x => x.LapCap).ToList();
    }

    public async Task<SingleLapRecord?> GetPilotOpenPracticeSessionSingleLapRecord(Guid sessionId, Guid pilotId)
    {
        var position = await _dbContext.SingleLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        return position == null
            ? null
            : new SingleLapRecord(position.TotalMilliseconds, position.CompletionUtc, position.LapId);
    }

    public async Task<TotalLapRecord?> GetPilotOpenPracticeSessionTotalLapRecord(Guid sessionId, Guid pilotId)
    {
        var position = await _dbContext.TotalLapsLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        return position == null
            ? null
            : new TotalLapRecord(position.TotalLaps, position.FirstLapCompletionUtc);
    }

    public async Task<PilotLaneConfiguration> GetLaneConfiguration(Guid sessionId, byte lane)
    {
        var config = await _dbContext.OpenPracticeLaneConfigurations.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane);

        return new PilotLaneConfiguration(lane, config?.PilotId);
    }
}
