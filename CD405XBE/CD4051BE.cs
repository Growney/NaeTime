using Gpio;
using System.Device.Gpio;

namespace CD405XBE;

public class CD4051BE : IMultiplexer
{
    private readonly IGpioPin _aAddressPin;
    private readonly IGpioPin _bAddressPin;
    private readonly IGpioPin _cAddressPin;
    public IGpioPin CommonPin { get; }

    public CD4051BE(IGpioPin aAddressPin, IGpioPin bAddressPin, IGpioPin cAddressPin, IGpioPin commonPin)
    {
        _aAddressPin = aAddressPin;
        _bAddressPin = bAddressPin;
        _cAddressPin = cAddressPin;
        CommonPin = commonPin;
    }

    public void SetAddress(byte address)
    {
        if ((address & 0x01) == address)
        {
            _aAddressPin.Write(PinValue.High);
        }
        else
        {
            _aAddressPin.Write(PinValue.Low);
        }
        if ((address & 0x02) == address)
        {
            _bAddressPin.Write(PinValue.High);
        }
        else
        {
            _bAddressPin.Write(PinValue.Low);
        }
        if ((address & 0x04) == address)
        {
            _cAddressPin.Write(PinValue.High);
        }
        else
        {
            _cAddressPin.Write(PinValue.Low);
        }
    }

    public void Initialise()
    {
        _aAddressPin.OpenPin(PinMode.Output, PinValue.Low);
        _bAddressPin.OpenPin(PinMode.Output, PinValue.Low);
        _cAddressPin.OpenPin(PinMode.Output, PinValue.Low);
    }
}