using EventDbLite.Abstractions;
using EventDbLite.Exceptions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Hardware;
using NaeTime.Hardware.Abstractions;
using NaeTime.Query.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Command;

public class DetectionCommandHandler(IAggregateRepository repository, ISoftwareTimer softwareTimer) : IDetectionCommandHandler
{
    public Task Move(Guid detectionId, long softwareTime, DateTime utcTime) => ConcurrencyException.Retry(async () =>
    {
        Aggregates.Detection aggregate = await repository.Get<Aggregates.Detection, Guid>(detectionId) ?? throw new InvalidOperationException("Detection not found");

        aggregate.Move(softwareTime, utcTime);

        await repository.Save(aggregate);
    });

    public Task OverridePilot(Guid detectionId, Guid pilotId) => ConcurrencyException.Retry(async () =>
    {
        Aggregates.Detection aggregate = await repository.Get<Aggregates.Detection, Guid>(detectionId) ?? throw new InvalidOperationException("Detection not found");

        aggregate.OverridePilot(pilotId);

        await repository.Save(aggregate);
    });

    public Task OverrideSession(Guid detectionId, Guid sessionId) => ConcurrencyException.Retry(async () =>
    {
        Aggregates.Detection aggregate = await repository.Get<Aggregates.Detection, Guid>(detectionId) ?? throw new InvalidOperationException("Detection not found");

        aggregate.OverrideSession(sessionId);

        await repository.Save(aggregate);
    });

    public Task SetStatus(Guid detectionId, bool isValid) => ConcurrencyException.Retry(async () =>
    {
        Aggregates.Detection aggregate = await repository.Get<Aggregates.Detection, Guid>(detectionId) ?? throw new InvalidOperationException("Detection not found");

        aggregate.SetStatus(isValid);

        await repository.Save(aggregate);
    });

    public Task Trigger(Guid id, Guid timerId, byte laneId, ulong hardwareTimer, long softwareTimer, DateTime utcTime)
    {
        Aggregates.Detection aggregate = repository.CreateNew<Aggregates.Detection>(() => new Aggregates.Detection(id, timerId, laneId, softwareTimer, utcTime));
        return repository.Save(aggregate);
    }

    public Task Trigger(Guid id, Guid timerId, byte laneId)
    {
        Aggregates.Detection aggregate = repository.CreateNew<Aggregates.Detection>(() => new Aggregates.Detection(id, timerId, laneId, softwareTimer.ElapsedMilliseconds, DateTime.UtcNow));
        return repository.Save(aggregate);
    }
}
