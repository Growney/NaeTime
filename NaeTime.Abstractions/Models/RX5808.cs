namespace NaeTime.Abstractions.Models
{
    public class RX5808
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public string? Identifier { get; set; }
        public bool UseAnalogToDigitalConverter { get; set; }
        public byte RSSIPin { get; set; }
        public byte DataPin { get; set; }
        public byte SelectPin { get; set; }
        public byte ClockPin { get; set; }

        public RssiBoundary Boundary { get; set; } = RssiBoundary.RX5808Default;
    }
}
