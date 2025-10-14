using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Command;
public class OpenPracticePilotTimingCommandHandler : IOpenPracticePilotTimingCommandHandler
{
    private readonly IAggregateRepository _repository;
    public OpenPracticePilotTimingCommandHandler(IAggregateRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
    public async Task AddDetectionToPilot(Guid pilotId, Guid sessionId, Guid detectionId, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        OpenPracticePilotTiming aggregate = await _
    }

    public Task MarkPilotDetectionAsInvalid(Guid pilotId, Guid sessionId, Guid detectionId)
    {
        throw new NotImplementedException();
    }

    public Task MarkPilotDetectionAsValid(Guid pilotId, Guid sessionId, Guid detectionId)
    {
        throw new NotImplementedException();
    }

    public Task RemoveDetectionFromPilot(Guid pilotId, Guid sessionId, Guid detectionId)
    {
        throw new NotImplementedException();
    }
}
