namespace NaeTime.Orchestrator.Distribution.Abstractions;

public static class IDistributionReceiverExtensions
{
    public static async Task Process<T>(this IDistributionReceiver receiver, CancellationToken token, Func<T, Task> handler)
    {
        while (!token.IsCancellationRequested)
        {
            T? item = await receiver.WaitForNextAsync<T>(token).ConfigureAwait(false);
            if (item == null)
            {
                break;
            }

            await handler(item);
        }
    }
}
