namespace NaeTime.Timing.Abstractions.Models;
public class Flight
{
    public Flight(IEnumerable<Flight> flights)
    {

        Flights = flights ?? throw new ArgumentNullException(nameof(flights));
    }

    public IEnumerable<Flight> Flights { get; }

}
