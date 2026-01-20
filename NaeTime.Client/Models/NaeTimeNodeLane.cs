namespace NaeTime.Client.Models;
public class NaeTimeNodeLane
{
    public byte LaneId { get; set; }
    public bool IsEnabled { get; set; }
    public ushort EntryThreshold { get; set; }
    public ushort ExitThreshold { get; set; }
    public byte? BandId { get; set; }
    public int FrequencyInMHz { get; set; }
}
