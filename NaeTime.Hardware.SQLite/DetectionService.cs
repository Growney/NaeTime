namespace NaeTime.Hardware.SQLite;
internal class DetectionService
{
    private readonly HardwareDbContext _dbContext;
    public DetectionService(HardwareDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task When(TimerDetectionOccured timerDetection)
    {
        _dbContext.Detections.Add(new Detection
        {
            TimerId = timerDetection.TimerId,
            Lane = timerDetection.Lane,
            HardwareTime = timerDetection.HardwareTime,
            SoftwareTime = timerDetection.SoftwareTime,
            UtcTime = timerDetection.UtcTime,
            IsManual = false
        });

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

    }
    public async Task When(TimerDetectionTriggered timerDetection)
    {
        _dbContext.Detections.Add(new Detection
        {
            TimerId = timerDetection.TimerId,
            Lane = timerDetection.Lane,
            SoftwareTime = timerDetection.SoftwareTime,
            UtcTime = timerDetection.UtcTime,
            IsManual = true
        });

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
