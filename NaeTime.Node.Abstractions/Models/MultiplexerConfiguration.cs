namespace NaeTime.Node.Abstractions.Models;

public class MultiplexerConfiguration
{
    public byte Id { get; set; }
    public byte AAddressPin { get; set; }
    public byte BAddressPin { get; set; }
    public byte CAddressPin { get; set; }

    public byte DataPin { get; set; }
    public byte SelectPin { get; set; }
    public byte ClockPin { get; set; }
}
