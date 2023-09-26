using Iot.Device.Adc;
using System.Collections.Concurrent;

namespace Gpio;

public class WrappedMcp3008 : IAnalogToDigitalConverter, IDisposable
{
    private readonly Mcp3008 _adc;

    private readonly ConcurrentDictionary<int, Mcp3008AnalogPin> _pins = new ConcurrentDictionary<int, Mcp3008AnalogPin>();

    public WrappedMcp3008(Mcp3008 adc)
    {
        _adc = adc;
    }

    public void Dispose()
    {
        _adc.Dispose();
    }

    public IAnalogPin GetPin(int channel) => _pins.GetOrAdd(channel, x => new Mcp3008AnalogPin(_adc, x));

}
