namespace Gpio;

public interface IAnalogToDigitalConverter
{
    IAnalogPin GetPin(int channel);
}
