using NaeTime.Node.Abstractions.Models;

namespace NaeTime.Node.Abstractions.Domain;

public interface INodeClient : IAsyncDisposable
{
    Task SendInitializedAsync(NodeConfiguration configuration);
    Task SendTimerStartAsync(Guid nodeId, Guid sessionId);
    Task SendTimerStoppedAsync(Guid nodeId, Guid sessionId);
    Task SendReadingsAsync(Guid nodeId, byte deviceId, int frequency, IEnumerable<RssiReading> readings);
}
