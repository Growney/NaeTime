using NaeTime.Client.Razor.Lib.Validation;
using System.ComponentModel.DataAnnotations;

namespace NaeTime.Client.Razor.Lib.Models;
public class EthernetLapRF8Channel
{
    public EthernetLapRF8Channel(Guid id, string name, string? ipAddress, int port)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        IpAddress = ipAddress;
        Port = port;
    }

    public Guid Id { get; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    [IpAddress]
    public string? IpAddress { get; set; }
    [Required]
    [Range(0, 65535)]
    public int Port { get; set; }
}
