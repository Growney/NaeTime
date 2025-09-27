namespace NaeTime.Client.Models;
public class ImmersionRCLapRFLane
{
    public byte LaneId { get; set; }
    public bool IsEnabled { get; set; }
    public ushort Gain { get; set; }
    public float Threshold { get; set; }
    public byte? BandId { get; set; }
    public int FrequencyInMHz { get; set; }
}
