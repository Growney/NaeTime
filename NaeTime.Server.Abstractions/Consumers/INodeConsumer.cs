using NaeTime.Server.Abstractions.Events;

namespace NaeTime.Server.Abstractions.Consumers;

public interface INodeConsumer
{
    Task When(RssiReadingGroupReceived readingGroup, IEnumerable<RssiReadingReceived> readings);
    Task When(NodeConfigured nodeConfiguration);
    Task When(NodeTimerStarted timerStarted);
    Task When(NodeTimerStopped timerStopped);
}
