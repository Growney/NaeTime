using NaeTime.Abstractions.Enumeration;

namespace NaeTime.Abstractions.Models
{
    public class TimedGate
    {
        public Guid Id { get; set; }
        public Guid TrackId { get; set; }
        public int Position { get; set; }
        public Guid NodeId { get; set; }
        public RssiReceiverType? RssiReceiverType { get; set; }
    }
}
