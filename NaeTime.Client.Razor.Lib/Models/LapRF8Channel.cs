namespace NaeTime.Client.Razor.Lib.Models;
public class LapRF8Channel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int IpAddress { get; set; }
    public ushort Port { get; set; }
}
