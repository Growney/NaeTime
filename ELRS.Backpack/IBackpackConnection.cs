namespace ELRS.Backpack;

public interface IBackpackConnection
{
    public Task ConnectAsync(CancellationToken token);

    public Task Run(CancellationToken token);

    public Task<byte> GetBandChannelIndex(byte[] UId);
    public Task SetBandChannelIndex(byte[] UId, byte channelIndex);
    public Task<ushort> GetFrequency(byte[] UId);
    public Task SetFrequency(byte[] UId, ushort frequencyInMHz);
    public Task<bool> GetRecordingState(byte[] UId);
    public Task SetRecordingState(byte[] UId, bool recording);
    public Task<byte> GetVRxMode(byte[] UId);
    public Task SetVRxMode(byte[] UId, byte mode);
    public Task<byte> GetRSSI(byte[] UId);
    public Task<ushort> GetBatteryVoltage(byte[] UId);
    public Task<byte[]> GetVRxVersion(byte[] UId);
    public Task SetBuzzer(byte[] UId, ushort durationMs);
    public Task SetOSDElement(byte[] UId, string message, OSDPresentation presentation, byte row, byte column, TimeSpan? duration);
    public Task SetHeadTrackingState(byte[] UId, bool state);
    public Task SetMode(BackpackMode mode);
    public Task<string> GetVersion();
    public Task<BackpackStatus> GetStatus();
    public Task SetHeadTrackingData(byte[] UId, ushort pan, ushort tilt, ushort roll);
}
