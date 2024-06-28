using Gpio;
using RX5808.Enumeration;
using System.Device.Gpio;
using System.Diagnostics;

namespace RX5808;

public class RX5808RegisterCommunication
{

    public const int c_minimumTransactionDelayInMilliseconds = 500;
    public const int c_communicationDelayInMicroseconds = 50;

    private readonly Stopwatch _stopwatch;
    private readonly IGpioPin _dataPin;
    private readonly IGpioPin _selectPin;
    private readonly IGpioPin _clockPin;

    private PinMode _dataPinMode;
    private long? _lastTransactionTime = null;

    internal RX5808RegisterCommunication(IGpioPin dataPin, IGpioPin selectPin, IGpioPin clockPin)
    {
        _dataPin = dataPin ?? throw new ArgumentNullException(nameof(dataPin));
        _selectPin = selectPin ?? throw new ArgumentNullException(nameof(selectPin));
        _clockPin = clockPin ?? throw new ArgumentNullException(nameof(clockPin));


        _stopwatch = Stopwatch.StartNew();
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

    public static bool DoTunedFrequenciesMatch(int frequencyOne, int frequencyTwo)
        => CalculateFrequencyRegisterValue(frequencyOne) == CalculateFrequencyRegisterValue(frequencyTwo);

    private void StampTransactionTime()
    {
        _lastTransactionTime = _stopwatch.ElapsedMilliseconds;
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
    private void WriteBit(bool value)
    {
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _dataPin.Write(value ? PinValue.High : PinValue.Low);
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _clockPin.Write(PinValue.High);
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _clockPin.Write(PinValue.Low);
    }
    private bool ReadBit()
    {
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        var pinValue = _dataPin.Read();
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _clockPin.Write(PinValue.High);
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _clockPin.Write(PinValue.Low);

        return pinValue == PinValue.High;
    }
    private void SetSelect(bool isHigh)
    {
        _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
        _selectPin.Write(isHigh ? PinValue.High : PinValue.Low);
    }
    private void SetupDataPin(PinMode mode)
    {
        if (_dataPinMode != mode)
        {
            _dataPinMode = mode;
            _stopwatch.DelayMicroseconds(c_communicationDelayInMicroseconds);
            _dataPin.SetPinMode(mode);
        }
    }
    private void SendRegisterAddress(Registers register)
    {
        for (byte i = 0; i < 4; i++)
        {
            bool readBit = (((byte)register >> i) & 0x01) == 0x01;
            WriteBit(readBit);
        }
    }
    private async Task<int> PerformReadTransactionAsync(Registers register)
    {
        await WaitForTransactionDelay();

        int result = await Task.Run(() => ReadFromRegister(register));
        StampTransactionTime();

        return result;
    }
    private int ReadFromRegister(Registers register)
    {
        SetSelect(true); //Raise the select before a transaction to ensure that the trailing edge proceeds the transaction
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

        StampTransactionTime();
    }
    private void WriteToRegister(Registers register, int value)
    {
        SetSelect(true); //Raise the select before a transaction to ensure that the trailing edge proceeds the transaction
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

    private static int CalculateFrequencyRegisterValue(int frequencyInMhz)
    {
        int tf = (frequencyInMhz - 479) / 2;
        int N = tf / 32;
        int A = tf % 32;
        return (N << 7) + A;
    }
    private static int CalculateRegisterValueFrequency(int registerValue)
    {
        int N = registerValue >> 7;
        int A = registerValue & 0x7F;

        int tf = (N * 32) + A;

        return (tf * 2) + 479;
    }
}
