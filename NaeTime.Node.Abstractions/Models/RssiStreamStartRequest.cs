namespace NaeTime.Node.Abstractions.Models;

public class RssiStreamStartRequest
{
    public RssiStreamStartRequest(Guid id, int frequency, int deviceId)
    {
        Id = id;
        Frequency = frequency;
        DeviceId = deviceId;
    }

    public Guid Id { get; }
    public int Frequency { get; }
    public int DeviceId { get; }

}
