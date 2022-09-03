namespace NaeTime.Abstractions.Models
{
    public class Track
    {
        public Guid Id { get; set; }
        public Guid CreatorPilotId { get; set; }
        public string? Name { get; set; }
        public long MinimumLapTime { get; set; }
        public List<TimedGate> Gates { get; set; } = new();

    }
}
