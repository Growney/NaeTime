using NaeTime.Events.Domain;
using NaeTime.Hardware.ELRS.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using System.Security.Cryptography;

namespace NaeTime.Hardware.ELRS;

internal class BackpackReactions
{
    private readonly IBackpackConnectorProvider _backpackConnectorProvider;
    private readonly IHardwareQueryHandler _hardwareQueryHandler;
    private readonly IPilotQueryHandler _pilotQueryHandler;
    private readonly ITimingQueryHandler _openPracticeQueryHandler;


    public BackpackReactions(IBackpackConnectorProvider backpackConnectorProvider, IHardwareQueryHandler hardwareQueryHandler, IPilotQueryHandler pilotQueryHandler, ITimingQueryHandler openPracticeQueryHandler)
    {
        _backpackConnectorProvider = backpackConnectorProvider ?? throw new ArgumentNullException(nameof(backpackConnectorProvider));
        _hardwareQueryHandler = hardwareQueryHandler ?? throw new ArgumentNullException(nameof(hardwareQueryHandler));
        _pilotQueryHandler = pilotQueryHandler ?? throw new ArgumentNullException(nameof(pilotQueryHandler));
        _openPracticeQueryHandler = openPracticeQueryHandler ?? throw new ArgumentNullException(nameof(openPracticeQueryHandler));
    }
    private Task When(OpenPracticePilotDetectionTriggered occured) => TriggerLap(occured.SessionId, occured.TrackId, occured.PilotId, occured.DetectionId);
    private Task When(OpenPracticePilotDetectionOccured occured) => TriggerLap(occured.SessionId, occured.TrackId, occured.PilotId, occured.DetectionId);

    private async Task TriggerLap(Guid sessionId, Guid trackId, Guid pilotId, Guid detectionId)
    {
        byte[]? uid = await _pilotQueryHandler.GetPilotBindingPhrase(pilotId);

        if (uid is null)
        {
            return;
        }

        SessionTimingInformation? timingInfo = await _openPracticeQueryHandler.GetTimingInformation(sessionId, trackId);

        if (timingInfo is null)
        {
            return;
        }

        if (!timingInfo.PilotLapGroups.TryGetValue(pilotId, out IEnumerable<IEnumerable<OpenPracticeLap>>? lapGroups))
        {
            return;
        }

        OpenPracticeLap? completedLap = lapGroups.SelectMany(x => x).Where(lap => lap.EndDetection.Id == detectionId).FirstOrDefault();

        IEnumerable<SerialELRSBackpackInterface> backpacks = await _hardwareQueryHandler.GetAllSerialELRSBackpackInterfaces();
        if (completedLap is null)
        {
            foreach (SerialELRSBackpackInterface backpack in backpacks)
            {
                IBackpackConnector? connector = _backpackConnectorProvider.GetBackpackConnector(backpack.Id);

                if (connector is null || !connector.IsConnected)
                {
                    continue;
                }

                await connector.SetOSDElement(uid, $"Lap Started", 1, 20, TimeSpan.FromSeconds(5));
            }
            return;
        }

        IEnumerable<OpenPracticeLap> lapGroup = lapGroups.FirstOrDefault(x => x.Select(openPracticeLap => openPracticeLap.EndDetection.Id).Contains(detectionId)) ?? Enumerable.Empty<OpenPracticeLap>();

        IEnumerable<OpenPracticeLap> filteredLaps = lapGroup.Reverse().Take(3);

        double totalTime = lapGroup.Sum(x => x.Duration.TotalSeconds);
        double totalLapsCount = lapGroup.Count();

        double filteredTime = filteredLaps.Sum(x => x.Duration.TotalSeconds);
        double filteredLapsCount = filteredLaps.Count();


        foreach (SerialELRSBackpackInterface backpack in backpacks)
        {
            IBackpackConnector? connector = _backpackConnectorProvider.GetBackpackConnector(backpack.Id);

            if (connector is null || !connector.IsConnected)
            {
                continue;
            }

            await connector.SetOSDElement(uid, $"Lap:{Math.Round(completedLap.Duration.TotalSeconds, 2)}", 1, 20, TimeSpan.FromSeconds(5));
            await connector.SetOSDElement(uid, $"C:{filteredLapsCount}/{Math.Round(filteredTime,2)} T:{totalLapsCount}/{Math.Round(totalTime, 2)}", 0, 16, TimeSpan.FromSeconds(5));
        }
    }
}
