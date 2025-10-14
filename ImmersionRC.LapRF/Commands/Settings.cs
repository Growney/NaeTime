namespace ImmersionRC.LapRF.Commands;
internal class Settings(byte pilotId, long realTimeClockTime, short statusFlag, byte[] name, short updatePeriodMilliseconds, byte saveSettings, int minimumLapTimeMilliseconds, byte isModuleEnabled) : CommandBase(pilotId, realTimeClockTime, statusFlag)
{
    public byte[] Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public short UpdatePeriodMilliseconds { get; } = updatePeriodMilliseconds;
    public byte SaveSettings { get; } = saveSettings;
    public int MinimumLapTimeMilliseconds { get; } = minimumLapTimeMilliseconds;
    public byte IsModuleEnabled { get; } = isModuleEnabled;
}
