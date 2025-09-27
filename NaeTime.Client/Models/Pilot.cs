namespace NaeTime.Client.Models;
public class Pilot
{
    public Guid Id { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? Callsign { get; set; }
    public bool HasBindingPhrase { get; set; }
    public string? BindingPhrase { get; set; }
}
