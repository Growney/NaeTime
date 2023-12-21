using NaeTime.Client.Razor.Lib.Validation;
using System.ComponentModel.DataAnnotations;

namespace NaeTime.Client.Razor.Lib.Models;
public class EthernetLapRF8Channel
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    [IpAddress]
    public string? IpAddress { get; set; }
    [Required]
    [Range(0, 65535)]
    public int Port { get; set; }
}
