using System.Collections.Concurrent;
using System.Device.Gpio;

namespace Gpio;

public class WrappedGpioController : IGpioController
{
    private readonly GpioController _controller;

    private ConcurrentDictionary<int, WrappedGpioPin> _pins = new ConcurrentDictionary<int, WrappedGpioPin>();
    public WrappedGpioController(GpioController controller)
    {
        _controller = controller;
    }
    public void Dispose()
    {
        foreach (var pin in _pins.Values)
        {
            pin.Dispose();
        }

        _controller.Dispose();
    }

    public IGpioPin GetPin(int pinNumber)
    {
        return _pins.GetOrAdd(pinNumber, x => new WrappedGpioPin(_controller, x));
    }
}
