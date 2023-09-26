namespace NaeTime.Node.Abstractions.Models;

public class RssiStreamRequestResult
{
    public RssiStreamRequestResult(Guid id, int requestedFrequency, int tunedFrequency, int deviceId, bool success)
    {
        Id = id;
        RequestedFrequency = requestedFrequency;
        TunedFrequency = tunedFrequency;
        DeviceId = deviceId;
        Success = success;
    }

    public Guid Id { get; }
    public int RequestedFrequency { get; }
    public int TunedFrequency { get; }
    public int DeviceId { get; }
    public bool Success { get; }
}
