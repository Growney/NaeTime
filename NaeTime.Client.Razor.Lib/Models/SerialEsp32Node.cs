using System.ComponentModel.DataAnnotations;

namespace NaeTime.Client.Razor.Lib.Models;
public class SerialEsp32Node
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    [Required]
    public string? Port { get; set; }
}
