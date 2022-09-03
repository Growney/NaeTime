namespace NaeTime.Abstractions.Models
{
    public class Flight
    {
        public Guid Id { get; set; }
        public int Frequency { get; set; }
        public Guid TrackId { get; set; }
        public Guid PilotId { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }

        public List<Lap> Laps { get; set; } = new();
        public List<Split> Splits { get; set; } = new();
        public List<RssiStream> RssiStreams { get; set; } = new();
    }
}
