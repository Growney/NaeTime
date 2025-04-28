using Moq;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Timing;
namespace NaeTime.Orchestrator.Tests;
public class TimingOrchestrator_AddDetection
{
    private readonly Mock<ITimingOrchestratorPersistence> _mockPersistence;
    private readonly ITimingOrchestrator _orchestrator;

    public TimingOrchestrator_AddDetection()
    {
        _mockPersistence = new Mock<ITimingOrchestratorPersistence>();
        _orchestrator = new TimingOrchestrator(
            _mockPersistence.Object,
            Mock.Of<INaeTimeOrchestratorDistribution>()
        );
    }

    [Fact]
    public async Task AddSessionDetection_ShouldCallAddUnassignedDetection_WhenLaneIsUnassigned()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var timerId = Guid.NewGuid();
        var timerIndex = 1;
        var lane = (byte)2;
        var hardwareTime = (ulong?)123456;
        var softwareTime = 7890L;
        var fixedUtcTime = new DateTime(2025, 4, 19, 12, 0, 0, DateTimeKind.Utc); // Fixed DateTime

        // Act
        await _orchestrator.AddSessionDetection(sessionId, trackId, timerId, null, timerIndex, lane, hardwareTime, softwareTime, fixedUtcTime);

        // Assert
        _mockPersistence.Verify(p => p.AddDetection(sessionId, trackId, timerId, timerIndex, lane, null, hardwareTime, softwareTime, fixedUtcTime), Times.Once);
    }
    [Fact]
    public async Task AddSessionDetection_ShouldHandleFirstDetectionForPilot()
    {
        // Arrange
        var detectionId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var timerId = Guid.NewGuid();
        var timerIndex = 1;
        var lane = (byte)2;
        var hardwareTime = (ulong?)123456;
        var softwareTime = 7890L;
        var fixedUtcTime = new DateTime(2025, 4, 19, 12, 0, 0, DateTimeKind.Utc); // Fixed DateTime
        var pilotId = Guid.NewGuid();

        // Mock no prior detections for the pilot
        _mockPersistence
            .Setup(p => p.GetSessionPilotDetections(sessionId, pilotId))
            .ReturnsAsync(Array.Empty<Detection>());

        // Mock no prior laps for the pilot
        _mockPersistence
            .Setup(p => p.GetPilotOpenPracticeSessionLaps(sessionId, pilotId))
            .ReturnsAsync(Array.Empty<Lap>());

        //Mock the AddDetection method to return a detection ID
        _mockPersistence
            .Setup(p => p.AddDetection(sessionId, trackId, timerId, timerIndex, lane, pilotId, hardwareTime, softwareTime, fixedUtcTime))
            .ReturnsAsync(detectionId);

        // Act
        await _orchestrator.AddSessionDetection(sessionId, trackId, timerId, pilotId, timerIndex, lane, hardwareTime, softwareTime, fixedUtcTime);

        // Assert
        _mockPersistence.Verify(p => p.GetSessionPilotDetections(sessionId, pilotId), Times.Once);
        _mockPersistence.Verify(p => p.GetPilotOpenPracticeSessionLaps(sessionId, pilotId), Times.Once);
        _mockPersistence.Verify(p => p.AddDetection(sessionId, trackId, timerId, timerIndex, lane, pilotId, hardwareTime, softwareTime, fixedUtcTime), Times.Once);
        _mockPersistence.Verify(p => p.AddLap(sessionId, pilotId, detectionId, LapStatus.Incomplete), Times.Once);
    }
    [Fact]
    public async Task AddSessionDetection_ShouldCompleteLap_WhenFirstLapExistsWithNullExitDetection()
    {
        // Arrange
        var newDetectionId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var timerId = Guid.NewGuid();
        var timerIndex = 1;
        var lane = (byte)2;
        var hardwareTime = (ulong?)123456;
        var softwareTime = 7890L;
        var fixedUtcTime = new DateTime(2025, 4, 19, 12, 0, 0, DateTimeKind.Utc); // Fixed DateTime
        var pilotId = Guid.NewGuid();

        // Mock prior detection for the pilot
        var firstDetection = new Detection(
            Guid.NewGuid(),
            sessionId,
            trackId,
            timerId,
            timerIndex,
            lane,
            pilotId,
            hardwareTime - 5000,
            softwareTime - 5000, // Simulate earlier detection
            fixedUtcTime.AddSeconds(-5)
        );

        _mockPersistence
            .Setup(p => p.GetSessionPilotDetections(sessionId, pilotId))
            .ReturnsAsync(new[] { firstDetection });

        // Mock an existing lap with a null exit detection
        var existingLap = new Lap(Guid.NewGuid(), sessionId, pilotId, firstDetection, null, LapStatus.Incomplete);

        _mockPersistence
            .Setup(p => p.GetPilotOpenPracticeSessionLaps(sessionId, pilotId))
            .ReturnsAsync(new[] { existingLap });

        // Mock the AddDetection method to return a new detection ID
        _mockPersistence
            .Setup(p => p.AddDetection(sessionId, trackId, timerId, timerIndex, lane, pilotId, hardwareTime, softwareTime, fixedUtcTime))
            .ReturnsAsync(newDetectionId);

        // Act
        await _orchestrator.AddSessionDetection(sessionId, trackId, timerId, pilotId, timerIndex, lane, hardwareTime, softwareTime, fixedUtcTime);

        // Assert
        _mockPersistence.Verify(p => p.GetSessionPilotDetections(sessionId, pilotId), Times.Once);
        _mockPersistence.Verify(p => p.GetPilotOpenPracticeSessionLaps(sessionId, pilotId), Times.Once);

        // Verify that the new detection is added
        _mockPersistence.Verify(p => p.AddDetection(sessionId, trackId, timerId, timerIndex, lane, pilotId, hardwareTime, softwareTime, fixedUtcTime), Times.Once);

        // Verify that the existing lap is updated with the new exit detection
        _mockPersistence.Verify(p => p.CompleteLap(existingLap.Id, newDetectionId, LapStatus.Valid), Times.Once);
        // Verify that a new lap is started 
        _mockPersistence.Verify(p => p.AddLap(sessionId, pilotId, newDetectionId, LapStatus.Incomplete), Times.Once);

    }
}