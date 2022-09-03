using System;
using System.Device.Gpio;
using System.Diagnostics;
using Gpio;
using RX5808.Enumeration;

namespace RX5808;
public class RX5808Module : IDisposable
{

    public const int c_minimumTransactionDelayInMilliseconds = 250;
    public const int c_communicationDelayInMicroseconds = 50;

    private readonly Stopwatch _stopwatch;
    private readonly IGpioController _gpioController;

    protected readonly byte _rssiPin;
    protected readonly byte _dataPin;
    protected readonly byte _selectPin;
    protected readonly byte _clockPin;

    private long? _lastTransactionTime = null;

    private PinMode _dataPinMode;

    public bool IsInitialised { get; private set; }

    public RX5808Module(IGpioController controller, byte rssiPin, byte dataPin, byte selectPin, byte clockPin)
    {
        _rssiPin = rssiPin;
        _dataPin = dataPin;
        _selectPin = selectPin;
        _clockPin = clockPin;

        _gpioController = controller;
        _stopwatch = Stopwatch.StartNew();
    }
    public void Initialise()
    {
        _gpioController.OpenPin(_selectPin, PinMode.Output, PinValue.High);
        _gpioController.OpenPin(_clockPin, PinMode.Output, PinValue.Low);
        _gpioController.OpenPin(_dataPin, PinMode.Output, PinValue.Low);

        _dataPinMode = PinMode.Output;
        _lastTransactionTime = _stopwatch.ElapsedMilliseconds;

        IsInitialised = true;
    }
    public virtual int ReadRssi()
    {
        return _gpioController.Read(_rssiPin) == PinValue.High ? int.MaxValue : 0;
    }

    public Task EnableOptions(PowerDownControlRegister requiredOptions) => PerformWriteTransactionAsync(Registers.PowerDownControl, ~(int)requiredOptions);
    public async Task<PowerDownControlRegister> GetEnabledOptions()
    {
        var result = await PerformReadTransactionAsync(Registers.PowerDownControl);
        return (PowerDownControlRegister)~result;
    }
    public void Dispose()
    {
        _gpioController.CheckAndClosePin(_selectPin);
        _gpioController.CheckAndClosePin(_clockPin);
        _gpioController.CheckAndClosePin(_dataPin);
        _stopwatch.Stop();
    }

    public Task SetFrequencyAsync(int frequencyInMhz)
    {
        var registerValue = CalculateFrequencyRegisterValue(frequencyInMhz);

        return PerformWriteTransactionAsync(Registers.SynthesizerB, registerValue);
    }
    public async Task<bool> ConfirmFrequencyAsync(int frequencyInMhz)
    {
        var registerValue = await PerformReadTransactionAsync(Registers.SynthesizerB);

        return registerValue == CalculateFrequencyRegisterValue(frequencyInMhz);
    }
    public async Task<int> GetActualStoredFrequencyAsync()
    {
        var registerValue = await PerformReadTransactionAsync(Registers.SynthesizerB);

        return CalculateRegisterValueFrequency(registerValue);
    }
    private void SetSelect(bool isHigh)
    {
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _gpioController.Write(_selectPin, isHigh ? PinValue.High : PinValue.Low);
    }
    private void WriteBit(bool value)
    {
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _gpioController.Write(_dataPin, value ? PinValue.High : PinValue.Low);
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _gpioController.Write(_clockPin, PinValue.High);
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _gpioController.Write(_clockPin, PinValue.Low);
    }
    private bool ReadBit()
    {
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        var pinValue = _gpioController.Read(_dataPin);
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _gpioController.Write(_clockPin, PinValue.High);
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _gpioController.Write(_clockPin, PinValue.Low);

        return pinValue == PinValue.High;
    }
    private void SetupDataPin(PinMode mode)
    {
        if (_dataPinMode != mode)
        {
            _dataPinMode = mode;
            _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
            _gpioController.SetPinMode(_dataPin, mode);
        }
    }
    private void SendRegisterAddress(Registers register)
    {
        for (byte i = 0; i < 4; i++)
        {
            bool readBit = (((byte)register >> i) & 0x01) == 0x01;
            if (readBit)
            {
            }
            WriteBit(readBit);
        }
    }
    private async Task<int> PerformReadTransactionAsync(Registers register)
    {
        await WaitForTransactionDelay();

        int result = await Task.Run(() => ReadFromRegister(register));
        _lastTransactionTime = _stopwatch.ElapsedMilliseconds;

        return result;
    }
    private async Task WaitForTransactionDelay()
    {
        if (_lastTransactionTime != null)
        {
            long timeSince = _stopwatch.ElapsedMilliseconds - _lastTransactionTime.Value;
            if (timeSince < c_minimumTransactionDelayInMilliseconds)
            {
                var timeToWait = c_minimumTransactionDelayInMilliseconds - timeSince;
                if (timeToWait > int.MaxValue)
                {
                    throw new InvalidOperationException("Invalid wait delay");
                }

                await Task.Delay((int)timeToWait);
            }

        }
    }
    private int ReadFromRegister(Registers register)
    {
        SetupDataPin(PinMode.Output);
        SetSelect(false);

        //Write register address
        SendRegisterAddress(register: register);

        //We are reading from register
        WriteBit(false);

        SetupDataPin(PinMode.InputPullUp);

        int result = 0;
        for (byte location = 0; location < 20; location++)
        {
            bool readValue = ReadBit();
            if (readValue)
            {
                result |= 1 << location;
            }
        }
        SetSelect(true);
        return result;
    }
    private async Task PerformWriteTransactionAsync(Registers register, int value)
    {
        await WaitForTransactionDelay();

        await Task.Run(() => WriteToRegister(register, value));

        _lastTransactionTime = _stopwatch.ElapsedMilliseconds;
    }
    private void WriteToRegister(Registers register, int value)
    {
        SetupDataPin(PinMode.Output);
        SetSelect(false);

        //Write register address
        SendRegisterAddress(register: register);

        WriteBit(true);

        for (byte location = 0; location < 20; location++)
        {
            if ((value & (1 << location)) == (1 << location))
            {
                WriteBit(true);
            }
            else
            {
                WriteBit(false);
            }
        }
        SetSelect(isHigh: true);
    }
    private int CalculateFrequencyRegisterValue(int frequencyInMhz)
    {
        int tf = (frequencyInMhz - 479) / 2;
        int N = tf / 32;
        int A = tf % 32;
        return (N << 7) + A;
    }
    private int CalculateRegisterValueFrequency(int registerValue)
    {
        int N = registerValue >> 7;
        int A = registerValue & 0x7F;

        int tf = (N * 32) + A;

        return (tf * 2) + 479;
    }
}
