namespace NaeTime.Hardware.ELRS.Abstractions;

public interface IBackpackConnector
{
    bool IsConnected { get; }
    Task Stop();
    Task SetOSDElement(byte[] uid, string text, byte row, byte column, TimeSpan duration);
}
