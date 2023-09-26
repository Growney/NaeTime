using Iot.Device.Adc;

namespace Gpio;

public class Mcp3008AnalogPin : IAnalogPin
{
    private readonly Mcp3008 _parent;
    private readonly int _channel;

    public Mcp3008AnalogPin(Mcp3008 parent, int channel)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _channel = channel;
    }

    public int ReadValue() => _parent.Read(_channel);
}
