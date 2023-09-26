using System.Device.Gpio;

namespace Gpio;

public class WrappedGpioPin : IGpioPin
{
    private readonly int _pinNumber;
    private readonly GpioController _parentController;

    public WrappedGpioPin(GpioController parentController, int pinNumber)
    {
        _parentController = parentController;
        _pinNumber = pinNumber;
    }

    public void ClosePin() => _parentController.ClosePin(_pinNumber);

    public void Dispose() => _parentController.ClosePin(_pinNumber);

    public bool IsPinOpen() => _parentController.IsPinOpen(_pinNumber);

    public void OpenPin(PinMode mode) => _parentController.OpenPin(_pinNumber, mode);

    public void OpenPin(PinMode mode, PinValue initialValue) => _parentController.OpenPin(_pinNumber, mode, initialValue);

    public PinValue Read() => _parentController.Read(_pinNumber);

    public void SetPinMode(PinMode mode) => _parentController.SetPinMode(_pinNumber, mode);

    public void Write(PinValue value) => _parentController?.Write(_pinNumber, value);
}
