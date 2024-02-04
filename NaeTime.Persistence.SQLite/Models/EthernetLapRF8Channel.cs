namespace NaeTime.Persistence.SQLite.Models;
public class EthernetLapRF8Channel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public byte[] IpAddress { get; set; }
    public int Port { get; set; }
}
