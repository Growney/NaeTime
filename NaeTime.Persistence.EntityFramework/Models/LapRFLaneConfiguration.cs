namespace NaeTime.Persistence.EntityFramework.Models;

public class LapRFLaneConfiguration
{
    public Guid Id { get; set; }
    public byte Lane { get; set; }
    public ushort? DesiredGain { get; set; }
    public float? DesiredThreshold { get; set; }
    public ushort? ActualGain { get; set; }
    public float? ActualThreshold { get; set; }
}
