using System.ComponentModel.DataAnnotations;

namespace NaeTime.Client.Razor.Lib.Models;

public class Pilot
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string? FirstName { get; set; }
    [Required]
    [MaxLength(50)]
    public string? LastName { get; set; }
    [Required]
    [MaxLength(50)]
    public string? CallSign { get; set; }
}
