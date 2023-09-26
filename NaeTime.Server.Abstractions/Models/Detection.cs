namespace NaeTime.Server.Abstractions.Models;
public class Detection
{
    public int FrequencyId { get; }
    public long Tick { get; }
    public Guid DetectorId { get; }
}
