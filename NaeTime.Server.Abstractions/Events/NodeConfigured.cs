namespace NaeTime.Server.Abstractions.Events;
public class NodeConfigured
{

    public class AnalogToDigitalConverterConfiguration
    {
        public enum AnalogToDigitalConverterModeDto
        {
            HardwareSPI,
        }
        public enum ADCSpiMode
        {
            /// <summary>
            /// CPOL 0, CPHA 0. Polarity is idled low and data is sampled on rising edge of the clock signal.
            /// </summary>
            Mode0,

            /// <summary>
            /// CPOL 0, CPHA 1. Polarity is idled low and data is sampled on falling edge of the clock signal.
            /// </summary>
            Mode1,

            /// <summary>
            /// CPOL 1, CPHA 0. Polarity is idled high and data is sampled on falling edge of the clock signal.
            /// </summary>
            Mode2,

            /// <summary>
            /// CPOL 1, CPHA 1. Polarity is idled high and data is sampled on rising edge of the clock signal.
            /// </summary>
            Mode3
        }
        public byte Id { get; set; }
        public AnalogToDigitalConverterModeDto Mode { get; set; } = AnalogToDigitalConverterModeDto.HardwareSPI;
        public int BusId { get; set; } = 0;
        public int ChipSelectLine { get; set; } = 0;
        public int ClockFrequency { get; set; } = 3_000_000;
        public ADCSpiMode SpiMode { get; set; } = ADCSpiMode.Mode1;
        public int DataBitLength { get; set; } = 8;
    }
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
    public class AnalogToDigitalConverterRX5808Configuration
    {
        public byte Id { get; set; }

        public byte DataPin { get; set; }
        public byte SelectPin { get; set; }
        public byte ClockPin { get; set; }

        public byte ADCId { get; set; }
        public byte ADCChannel { get; set; }

        public int Frequency { get; set; }
        public bool IsEnabled { get; set; }
    }

    public Guid NodeId { get; set; }
    public string? ServerAddress { get; set; }
    public IEnumerable<AnalogToDigitalConverterConfiguration> AnalogToDigitalConverters { get; set; } = Enumerable.Empty<AnalogToDigitalConverterConfiguration>();
    public IEnumerable<MultiplexerConfiguration> MultiplexerConfigurations { get; set; } = Enumerable.Empty<MultiplexerConfiguration>();
    public IEnumerable<MultiplexedAnalogToDigitalConverterRx5808Configuration> MultiplexedRX5808Configurations { get; set; } = Enumerable.Empty<MultiplexedAnalogToDigitalConverterRx5808Configuration>();
    public IEnumerable<AnalogToDigitalConverterRX5808Configuration> RX5808Configurations { get; set; } = Enumerable.Empty<AnalogToDigitalConverterRX5808Configuration>();
}
