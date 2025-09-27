namespace NaeTime.Client.Models;
public class ImmersionRCLapRF
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public List<ImmersionRCLapRFLane> Lanes { get; set; } = new();
}
