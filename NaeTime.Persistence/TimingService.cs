using NaeTime.Messages.Events.Timing;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub;

namespace NaeTime.Persistence;
public class TimingService : ISubscriber
{
    private readonly IRepositoryFactory _repositoryFactory;

    public TimingService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }


    public async Task When(TimerDetectionOccured timerDetection)
    {
        var timingRepository = await _repositoryFactory.CreateTimingRepository().ConfigureAwait(false);
        await timingRepository.AddTimerDetection(timerDetection.TimerId, timerDetection.Lane, timerDetection.HardwareTime, timerDetection.SoftwareTime, timerDetection.UtcTime).ConfigureAwait(false);
    }
    public async Task When(TimerDetectionTriggered timerDetection)
    {
        var timingRepository = await _repositoryFactory.CreateTimingRepository().ConfigureAwait(false);
        await timingRepository.AddManualDetection(timerDetection.TimerId, timerDetection.Lane, timerDetection.SoftwareTime, timerDetection.UtcTime).ConfigureAwait(false);
    }
}
