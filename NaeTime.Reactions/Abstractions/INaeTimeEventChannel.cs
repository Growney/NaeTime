using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Reactions.Abstractions;

public interface IEventChannel
{
    ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
    bool TryPublish<TEvent>(TEvent @event);
    IAsyncEnumerable<TEvent> SubscribeAsync<TEvent>(CancellationToken cancellationToken = default);
}