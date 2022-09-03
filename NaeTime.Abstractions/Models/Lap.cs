namespace NaeTime.Abstractions.Models
{
    /// <summary>
    /// A lap represents a section of a flight between the first timed gate
    /// </summary>
    public class Lap
    {
        public Guid Id { get; set; }
        public Guid FlightId { get; set; }
        public long StartTick { get; set; }
        public long? EndTick { get; set; }
    }
}
