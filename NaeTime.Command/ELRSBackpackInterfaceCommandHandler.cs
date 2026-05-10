using EventDbLite.Abstractions;
using EventDbLite.Exceptions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Command;

public class ELRSBackpackInterfaceCommandHandler(IAggregateRepository _repository) : IELRSBackpackInterfaceCommandHandler
{
    public Task Add(Guid id, string name, string comPort) => ConcurrencyException.Retry(() =>
    {
        ELRSBackpackInterface aggregate = _repository.CreateNew<ELRSBackpackInterface>(() => new ELRSBackpackInterface(id, name, comPort));
        return _repository.Save(aggregate);
    });

    public Task ReconfigureComPort(Guid id, string comPort) => ConcurrencyException.Retry(async () =>
    {
        ELRSBackpackInterface aggregate = await _repository.Get<ELRSBackpackInterface, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.ConfigureComPort(comPort);
        await _repository.Save(aggregate);

    });

    public Task Rename(Guid id, string name) => ConcurrencyException.Retry(async () =>
    {
        ELRSBackpackInterface aggregate = await _repository.Get<ELRSBackpackInterface, Guid>(id) ?? throw new ArgumentException("Aggregate not found", nameof(id));
        aggregate.Rename(name);
        await _repository.Save(aggregate);

    });
}
