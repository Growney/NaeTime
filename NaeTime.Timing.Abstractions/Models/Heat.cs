namespace NaeTime.Timing.Abstractions.Models;
public class Heat
{
    public Guid Id { get; set; }
    public IEnumerable<Flight> Flights { get; set; }
}
