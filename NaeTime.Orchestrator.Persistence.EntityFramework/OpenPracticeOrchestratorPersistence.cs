using Microsoft.EntityFrameworkCore;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.EntityFramework;
using NaeTime.Persistence.EntityFramework.Models;

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
        OpenPracticeLaneConfiguration? laneConfiguration = await _dbContext.OpenPracticeLaneConfigurations.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new OpenPracticeLaneConfiguration
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
        OpenPracticeLaneConfiguration? laneConfiguration = await _dbContext.OpenPracticeLaneConfigurations.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new OpenPracticeLaneConfiguration
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
        OpenPracticeLaneConfiguration? laneConfiguration = await _dbContext.OpenPracticeLaneConfigurations.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new OpenPracticeLaneConfiguration
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
}
