namespace NaeTime.Client.Razor.Lib.SQlite.Models;
public class EthernetLapRF8ChannelDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public int Port { get; set; }
}
