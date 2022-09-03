namespace NaeTime.Abstractions.Models
{

    public class RssiBoundary
    {
        public static readonly RssiBoundary RX5808Default = new RssiBoundary() { PeakStartRssi = 240, PeakEndRssi = 240 };

        public int Id { get; set; }
        public int PeakStartRssi { get; set; }
        public int PeakEndRssi { get; set; }

    }
}
