using NaeTime.Abstractions.Enumeration;

namespace NaeTime.Abstractions.Models
{
    public class RssiStream
    {
        public Guid Id { get; set; }
        public Guid FlightId { get; set; }
        public Guid NodeId { get; set; }
        public Guid ReceiverId { get; set; }
        public RssiReceiverType ReceiverType { get; }
        public long StartTick { get; }
        public int TunedFrequency { get; set; }
        public RssiBoundary? Boundary { get; set; }
        public List<RssiStreamPass> Passes { get; set; } = new();
        public List<RssiStreamReading> Readings { get; set; } = new();
    }
}
