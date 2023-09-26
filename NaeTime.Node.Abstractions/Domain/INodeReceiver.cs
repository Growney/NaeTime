using NaeTime.Node.Abstractions.Enumeration;
using NaeTime.Node.Abstractions.Models;

namespace NaeTime.Node.Abstractions.Domain;

public interface INodeReceiver
{
    void OnServerAvailabilityChanged(Func<ServerAvailability> callback);
    void OnReceiverTune(Func<int, byte, Task<TuningResult>> callback);
    void OnReceiverDisable(Func<int, Task> callback);
    void OnReceiverEnable(Func<int, Task> callback);
    void OnTimerReset(Func<Task> callback);
}
