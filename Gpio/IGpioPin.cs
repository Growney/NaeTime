using System.Device.Gpio;

namespace Gpio;

public interface IGpioPin : IDisposable
{
    bool IsPinOpen();
    void OpenPin(PinMode mode);
    void OpenPin(PinMode mode, PinValue initialValue);
    void SetPinMode(PinMode mode);
    void Write(PinValue value);
    PinValue Read();
    void ClosePin();
}
