using Microsoft.Extensions.Hosting;
using NaeTime.Server.Abstractions.Hardware;

namespace NaeTime.Server.Hardware;
public class DetectionService : IHostedService
{
    private readonly IEnumerable<IDetectorFactory> _detectorFactories;
    private readonly IDetectionHandler _detectionHandler;
    private readonly List<Task> _detectionTask = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public DetectionService(IEnumerable<IDetectorFactory> detectorFactories, IDetectionHandler detectionHandler)
    {
        _detectorFactories = detectorFactories;
        _detectionHandler = detectionHandler;
    }

    private async Task RunDetectorAsync(IDetector detector, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var detection = await detector.WaitForNextDetectionAsync(cancellationToken);
            await _detectionHandler.HandleDetectionAsync(detection);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var detectorFactory in _detectorFactories)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                break;
            }
            var detectors = detectorFactory.CreateDetectors();
            foreach (var detector in detectors)
            {
                var detectorTask = RunDetectorAsync(detector, _cancellationTokenSource.Token);
                _detectionTask.Add(detectorTask);
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();

        return Task.CompletedTask;
    }

}
