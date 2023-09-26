namespace Gpio;

public interface IGpioController : IDisposable
{
    IGpioPin GetPin(int pinNumber);
}