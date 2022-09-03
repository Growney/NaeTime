
using System.Device.Gpio;

namespace Gpio;

public interface IGpioController
{
    bool IsPinOpen(int pinNumber);
    void OpenPin(int pinNumber, PinMode mode);
    void OpenPin(int pinNumber, PinMode mode, PinValue initialValue);
    void SetPinMode(int pinNumber, PinMode mode);
    void Write(int pinNumber, PinValue value);
    PinValue Read(int pinNumber);
    void ClosePin(int pinNumber);
}