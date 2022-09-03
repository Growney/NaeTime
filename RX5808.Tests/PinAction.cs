using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RX5808.Tests;

public struct PinAction
{
    private PinAction(long onTick, PinActionType type, int pinNumber, PinValue? value, PinMode? mode)
    {
        OnTick = onTick;
        Type = type;
        PinNumber = pinNumber;
        Value = value;
        Mode = mode;
    }

    public static PinAction OpenAndInitialise(int pin, PinMode mode, PinValue initialValue) => new PinAction(0, PinActionType.OpenPin, pin, initialValue, mode);
    public static PinAction Open(int pin, PinMode mode) => new PinAction(0, PinActionType.OpenPin, pin, null, mode);
    public static PinAction Read(int pin) => new PinAction(0, PinActionType.Read, pin, null, null);
    public static PinAction Write(int pin, PinValue value) => new PinAction(0, PinActionType.Write, pin, value, null);
    public static PinAction ChangeMode(int pin, PinMode mode) => new PinAction(0, PinActionType.ChangeMode, pin, null, mode);
    public static PinAction OpenAndInitialise(long onTick, int pin, PinMode mode, PinValue initialValue) => new PinAction(onTick, PinActionType.OpenPin, pin, initialValue, mode);
    public static PinAction Open(long onTick, int pin, PinMode mode) => new PinAction(onTick, PinActionType.OpenPin, pin, null, mode);
    public static PinAction Read(long onTick, int pin) => new PinAction(onTick, PinActionType.Read, pin, null, null);
    public static PinAction Write(long onTick, int pin, PinValue value) => new PinAction(onTick, PinActionType.Write, pin, value, null);
    public static PinAction ChangeMode(long onTick, int pin, PinMode mode) => new PinAction(onTick, PinActionType.ChangeMode, pin, null, mode);

    public long OnTick { get; }
    public PinActionType Type { get; }
    public int PinNumber { get; }
    public PinValue? Value { get; }
    public PinMode? Mode { get; }

    public override string ToString()
    => Type switch
    {
        PinActionType.Read => $"Read Pin {PinNumber}",
        PinActionType.Write => $"Write {Value!.Value} to Pin {PinNumber}",
        PinActionType.ChangeMode => $"Change Pin {PinNumber} to {Mode!.Value}",
        PinActionType.OpenPin when Value == null => $"Open pin {PinNumber} as {Mode}",
        PinActionType.OpenPin => $"Open pin {PinNumber} as {Mode} and initialise to {Value}",
        _ => "Unknown"
    };
}
