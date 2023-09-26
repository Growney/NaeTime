namespace ImmersionRC.LapRF.Commands;
internal class Settings : CommandBase
{
    public Settings(byte pilotId, long realTimeClockTime, short statusFlag, byte[] name, short updatePeriodMilliseconds, byte saveSettings, int minimumLapTimeMilliseconds, byte isModuleEnabled)
        : base(pilotId, realTimeClockTime, statusFlag)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        UpdatePeriodMilliseconds = updatePeriodMilliseconds;
        SaveSettings = saveSettings;
        MinimumLapTimeMilliseconds = minimumLapTimeMilliseconds;
        IsModuleEnabled = isModuleEnabled;
    }

    public byte[] Name { get; }
    public short UpdatePeriodMilliseconds { get; }
    public byte SaveSettings { get; }
    public int MinimumLapTimeMilliseconds { get; }
    public byte IsModuleEnabled { get; }
}
