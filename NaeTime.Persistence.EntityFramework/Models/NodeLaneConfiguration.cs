namespace NaeTime.Persistence.EntityFramework.Models;

public class NodeLaneConfiguration
{
    public Guid Id { get; set; }
    public byte Lane { get; set; }
    public int? ActualFrequency { get; set; }
    public ushort? DesiredEntryThreshold { get; set; }
    public ushort? DesiredExitThreshold { get; set; }
    public ushort? ActualEntryThreshold { get; set; }
    public ushort? ActualExitThreshold { get; set; }
}
