using Gpio;
using System.Device.Gpio;

namespace RX5808;

public class MultiplexedRX5808RegisterCommunication
{
    private const long c_minimumInteractionDelay = 10;

    private readonly IGpioPin _aAddressPin;
    private readonly IGpioPin _bAddressPin;
    private readonly IGpioPin _cAddressPin;

    private readonly IGpioPin _dataPin;
    private readonly IGpioPin _selectPin;
    private readonly IGpioPin _clockPin;

    private readonly SemaphoreSlim _semaphore;

    private readonly Dictionary<byte, RX5808RegisterCommunication> _devices = new();


    public MultiplexedRX5808RegisterCommunication(IGpioPin aAddressPin, IGpioPin bAddressPin, IGpioPin cAddressPin, IGpioPin dataPin, IGpioPin selectPin, IGpioPin clockPin)
    {
        _aAddressPin = aAddressPin;
        _bAddressPin = bAddressPin;
        _cAddressPin = cAddressPin;

        _dataPin = dataPin ?? throw new ArgumentNullException(nameof(dataPin));
        _selectPin = selectPin ?? throw new ArgumentNullException(nameof(selectPin));
        _clockPin = clockPin ?? throw new ArgumentNullException(nameof(clockPin));

        _semaphore = new SemaphoreSlim(1);
    }

    private RX5808RegisterCommunication GetAddressCommunication(byte address)
    {
        if (!_devices.TryGetValue(address, out var communication))
        {
            communication = new RX5808RegisterCommunication(_dataPin, _selectPin, _clockPin);
            _devices.Add(address, communication);
        }

        return communication;
    }

    public async Task<T> GetResult<T>(byte address, Func<RX5808RegisterCommunication, Task<T>> action)
    {
        await _semaphore.WaitAsync();

        SetAddress(address);

        await Task.Delay(10);

        var communication = GetAddressCommunication(address);

        T result = await action(communication);

        _semaphore.Release();

        return result;

    }
    public async Task PerformAction(byte address, Func<RX5808RegisterCommunication, Task> action)
    {
        await _semaphore.WaitAsync();


        SetAddress(address);

        await Task.Delay(10);

        var communication = GetAddressCommunication(address);

        await action(communication);

        _semaphore.Release();
    }
    private static bool IsBitSet(byte b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }
    private void SetAddress(byte address)
    {
        if (IsBitSet(address, 0))
        {
            _aAddressPin.Write(PinValue.High);
        }
        else
        {
            _aAddressPin.Write(PinValue.Low);
        }
        if (IsBitSet(address, 1))
        {
            _bAddressPin.Write(PinValue.High);
        }
        else
        {
            _bAddressPin.Write(PinValue.Low);
        }
        if (IsBitSet(address, 2))
        {
            _cAddressPin.Write(PinValue.High);
        }
        else
        {
            _cAddressPin.Write(PinValue.Low);
        }
    }
}
