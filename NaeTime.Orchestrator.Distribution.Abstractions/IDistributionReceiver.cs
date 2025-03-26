namespace NaeTime.Orchestrator.Distribution.Abstractions;

public interface IDistributionReceiver
{
    ValueTask<T?> WaitForNextAsync<T>(CancellationToken token);
}
