namespace NaeTime.Hardware.Node.Esp32.Abstractions;

public interface INodeConnection
{
    bool IsConnected { get; }

    Task SetLaneEnabled(byte Lane, bool isEnabled);
    Task SetLaneEntryThreshold(byte Lane, ushort threshold);
    Task SetLaneExitThreshold(byte Lane, ushort threshold);
    Task SetLaneRadioFrequency(byte Lane, int frequencyInMhz);
    Task Stop();
}