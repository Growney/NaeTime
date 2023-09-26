namespace NaeTime.Server.Abstractions.Events;

public class RssiReadingGroupReceived
{
    public Guid GroupId { get; set; }
    public Guid NodeId { get; set; }
    public byte DeviceId { get; set; }
    public int FrequencyId { get; set; }
}
