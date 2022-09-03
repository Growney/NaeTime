namespace NaeTime.Abstractions.Models
{
    /// <summary>
    /// A split represents a section of a flight between two timed gates of a track
    /// </summary>
    public class Split
    {
        public Guid Id { get; set; }
        public Guid FlightId { get; set; }
        public int Position { get; set; }
        public Guid StartGate { get; set; }
        public long StartTick { get; set; }
        public Guid EndGate { get; set; }
        public long? EndTick { get; set; }
    }
}
