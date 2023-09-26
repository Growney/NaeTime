namespace NaeTime.Node.Abstractions.Models;

public class MultiplexedAnalogToDigitalConverterRx5808Configuration
{
    public byte Id { get; set; }

    public byte MultiplexerId { get; set; }
    public byte MultiplexerChannel { get; set; }

    public byte ADCId { get; set; }
    public byte ADCChannel { get; set; }

    public int Frequency { get; set; }
    public bool IsEnabled { get; set; }
}
