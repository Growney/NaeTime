using Gpio;
using NaeTime.Node.Abstractions.Domain;

namespace NaeTime.Node.Domain;

public class AnalogToDigitalConverterRssiCommunication : IRssiCommunication
{
    private readonly IAnalogToDigitalConverter _adc;
    private readonly int _channel;

    private IAnalogPin _pin;

    public AnalogToDigitalConverterRssiCommunication(IAnalogToDigitalConverter adc, int channel)
    {
        _adc = adc ?? throw new ArgumentNullException(nameof(adc));
        _channel = channel;
        _pin = _adc.GetPin(_channel);
    }

    public int ReadRssi() => _pin.ReadValue();
}
