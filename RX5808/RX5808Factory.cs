using Gpio;
using System.Device.Gpio;

namespace RX5808;

public class RX5808Factory : IRX5808Factory
{
    private readonly IGpioController _gpioController;

    public RX5808Factory(IGpioController gpioController)
    {
        _gpioController = gpioController ?? throw new ArgumentNullException(nameof(gpioController));
    }

    public RX5808RegisterCommunication GetSingleModuleCommunication(byte dataPin, byte selectPin, byte clockPin)
    {

        var data = _gpioController.GetPin(dataPin);
        var select = _gpioController.GetPin(selectPin);
        var clock = _gpioController.GetPin(clockPin);

        data.OpenPin(PinMode.Output, PinValue.High);
        select.OpenPin(PinMode.Output, PinValue.Low);
        clock.OpenPin(PinMode.Output, PinValue.Low);

        return new RX5808RegisterCommunication(data, select, clock);
    }

    public MultiplexedRX5808RegisterCommunication GetMultiplexedModule(byte aAddressPin, byte bAddressPin, byte cAddressPin, byte dataPin, byte selectPin, byte clockPin)
    {
        var data = _gpioController.GetPin(dataPin);
        var select = _gpioController.GetPin(selectPin);
        var clock = _gpioController.GetPin(clockPin);

        var aAddress = _gpioController.GetPin(aAddressPin);
        var bAddress = _gpioController.GetPin(bAddressPin);
        var cAddress = _gpioController.GetPin(cAddressPin);

        data.OpenPin(PinMode.Output, PinValue.High);
        select.OpenPin(PinMode.Output, PinValue.Low);
        clock.OpenPin(PinMode.Output, PinValue.Low);

        aAddress.OpenPin(PinMode.Output, PinValue.Low);
        bAddress.OpenPin(PinMode.Output, PinValue.Low);
        cAddress.OpenPin(PinMode.Output, PinValue.Low);

        return new MultiplexedRX5808RegisterCommunication(aAddress, bAddress, cAddress, data, select, clock);

    }
}
