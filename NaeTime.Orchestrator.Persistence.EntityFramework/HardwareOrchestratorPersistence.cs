using Microsoft.EntityFrameworkCore;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Hardware;
using NaeTime.Persistence.EntityFramework;
using System.Net;

namespace NaeTime.Orchestrator.Persistence.EntityFramework;

public class HardwareOrchestratorPersistence : IHardwareOrchestratorPersistence
{
    private readonly NaeTimeDbContext _dbContext;
    public HardwareOrchestratorPersistence(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ConfigureDesiredLapRFLaneGain(Guid timerId, byte lane, ushort gain)
    {
        NaeTime.Persistence.EntityFramework.Models.LapRFLaneConfiguration? laneConfiguration = await _dbContext.LapRFConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.LapRFLaneConfiguration()
            {
                Id = timerId,
                Lane = lane,
                DesiredGain = gain
            };
            _dbContext.LapRFConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.DesiredGain = gain;
        }
    }
    public async Task ConfigureDesiredLapRFLaneThreshold(Guid timerId, byte lane, float threshold)
    {
        NaeTime.Persistence.EntityFramework.Models.LapRFLaneConfiguration? laneConfiguration = await _dbContext.LapRFConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.LapRFLaneConfiguration()
            {
                Id = timerId,
                Lane = lane,
                DesiredThreshold = threshold
            };
            _dbContext.LapRFConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.DesiredThreshold = threshold;
        }
    }
    public async Task ConfigureDesiredNodeEntryThreshold(Guid timerId, byte lane, ushort threshold)
    {
        NaeTime.Persistence.EntityFramework.Models.NodeLaneConfiguration? laneConfiguration = await _dbContext.NodeLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.NodeLaneConfiguration()
            {
                Id = timerId,
                Lane = lane,
                DesiredEntryThreshold = threshold
            };
            _dbContext.NodeLaneConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.DesiredEntryThreshold = threshold;
        }
    }
    public async Task ConfigureDesiredNodeExitThreshold(Guid timerId, byte lane, ushort threshold)
    {
        NaeTime.Persistence.EntityFramework.Models.NodeLaneConfiguration? laneConfiguration = await _dbContext.NodeLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.NodeLaneConfiguration()
            {
                Id = timerId,
                Lane = lane,
                DesiredExitThreshold = threshold
            };
            _dbContext.NodeLaneConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.DesiredExitThreshold = threshold;
        }
    }
    public async Task ConfigureDesiredTimerLaneRadioFrequency(Guid timerId, byte lane, byte? bandId, int frequencyInMhz)
    {
        NaeTime.Persistence.EntityFramework.Models.TimerLaneConfiguration? timerLaneStatus = await _dbContext.TimerLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.LaneId == lane);
        if (timerLaneStatus == null)
        {
            timerLaneStatus = new NaeTime.Persistence.EntityFramework.Models.TimerLaneConfiguration()
            {
                Id = timerId,
                DesiredBandId = bandId,
                DesiredFrequencyInMhz = frequencyInMhz
            };
            _dbContext.TimerLaneConfigurations.Add(timerLaneStatus);
        }
        else
        {
            timerLaneStatus.DesiredBandId = bandId;
            timerLaneStatus.DesiredFrequencyInMhz = frequencyInMhz;
        }
    }
    public async Task ConfigureDesiredTimerLaneStatus(Guid timerId, byte lane, bool isEnabled)
    {
        NaeTime.Persistence.EntityFramework.Models.TimerLaneConfiguration? timerLaneStatus = await _dbContext.TimerLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.LaneId == lane);

        if (timerLaneStatus == null)
        {
            timerLaneStatus = new NaeTime.Persistence.EntityFramework.Models.TimerLaneConfiguration()
            {
                Id = timerId,
                DesiredIsEnabled = isEnabled
            };
            _dbContext.TimerLaneConfigurations.Add(timerLaneStatus);
        }
        else
        {
            timerLaneStatus.DesiredIsEnabled = isEnabled;
        }
    }
    public async Task<bool> ConfigureEthernetLapRF8(Guid id, string name, IPAddress address, int port)
    {
        NaeTime.Persistence.EntityFramework.Models.EthernetLapRF8Channel? timer = await _dbContext.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == id);
        if (timer == null)
        {
            return false;
        }

        timer.Name = name;
        timer.IpAddress = address.GetAddressBytes();
        timer.Port = port;
        return true;
    }
    public async Task<bool> ConfigureSerialEsp32Node(Guid id, string name, string comPort)
    {
        NaeTime.Persistence.EntityFramework.Models.SerialEsp32Node? timer = await _dbContext.SerialEsp32Nodes.FirstOrDefaultAsync(x => x.Id == id);
        if (timer == null)
        {
            return false;
        }

        timer.Name = name;
        timer.Port = comPort;
        return true;
    }
    public async Task ConfigureSystemDesiredLaneRadioFrequency(byte lane, byte? bandId, int frequencyInMhz)
    {
        NaeTime.Persistence.EntityFramework.Models.SystemLaneConfiguration? laneConfiguration = await _dbContext.SystemLaneConfigurations.FirstOrDefaultAsync(x => x.LaneId == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.SystemLaneConfiguration()
            {
                Id = Guid.NewGuid(),
                LaneId = lane,
                BandId = bandId,
                Frequency = frequencyInMhz
            };
            _dbContext.SystemLaneConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.Frequency = frequencyInMhz;
        }
    }
    public async Task ConfigureSystemDesiredLaneStatus(byte lane, bool isEnabled)
    {
        NaeTime.Persistence.EntityFramework.Models.SystemLaneConfiguration? laneConfiguration = await _dbContext.SystemLaneConfigurations.FirstOrDefaultAsync(x => x.LaneId == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.SystemLaneConfiguration()
            {
                Id = Guid.NewGuid(),
                LaneId = lane,
                IsEnabled = isEnabled
            };
            _dbContext.SystemLaneConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.IsEnabled = isEnabled;
        }
    }
    public Task ConnectTimer(Guid timerId, DateTime connectionTime)
    {
        _dbContext.TimerStatuses.Add(new NaeTime.Persistence.EntityFramework.Models.TimerStatus()
        {
            Id = Guid.NewGuid(),
            ConnectionStatusChanged = connectionTime,
            TimerId = timerId,
            WasConnected = true
        });

        return Task.CompletedTask;
    }
    public Task<Guid> CreateEthernetLapRF8(string name, IPAddress address, int port)
    {
        NaeTime.Persistence.EntityFramework.Models.EthernetLapRF8Channel timer = new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            IpAddress = address.GetAddressBytes(),
            Port = port
        };
        _dbContext.EthernetLapRF8Channels.Add(timer);
        return Task.FromResult(timer.Id);
    }
    public Task<Guid> CreateSerialEsp32Node(string name, string comPort)
    {
        NaeTime.Persistence.EntityFramework.Models.SerialEsp32Node timer = new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Port = comPort
        };
        _dbContext.SerialEsp32Nodes.Add(timer);
        return Task.FromResult(timer.Id);
    }
    public Task DisconnectTimer(Guid timerId, DateTime disconnectionTime)
    {
        _dbContext.TimerStatuses.Add(new NaeTime.Persistence.EntityFramework.Models.TimerStatus()
        {
            Id = Guid.NewGuid(),
            ConnectionStatusChanged = disconnectionTime,
            TimerId = timerId,
            WasConnected = false
        });

        return Task.CompletedTask;
    }
    public async Task StoreActualLapRFLaneGain(Guid timerId, byte lane, ushort gain)
    {
        NaeTime.Persistence.EntityFramework.Models.LapRFLaneConfiguration? laneConfiguration = await _dbContext.LapRFConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.LapRFLaneConfiguration()
            {
                Id = timerId,
                Lane = lane,
                ActualGain = gain
            };
            _dbContext.LapRFConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.ActualGain = gain;
        }
    }
    public async Task StoreActualLapRFLaneThreshold(Guid timerId, byte lane, float threshold)
    {
        NaeTime.Persistence.EntityFramework.Models.LapRFLaneConfiguration? laneConfiguration = await _dbContext.LapRFConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.LapRFLaneConfiguration()
            {
                Id = timerId,
                Lane = lane,
                ActualThreshold = threshold
            };
            _dbContext.LapRFConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.ActualThreshold = threshold;
        }
    }
    public async Task StoreActualNodeEntryThreshold(Guid timerId, byte lane, ushort threshold)
    {
        NaeTime.Persistence.EntityFramework.Models.NodeLaneConfiguration? laneConfiguration = await _dbContext.NodeLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.NodeLaneConfiguration()
            {
                Id = timerId,
                Lane = lane,
                ActualEntryThreshold = threshold
            };
            _dbContext.NodeLaneConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.ActualEntryThreshold = threshold;
        }
    }
    public async Task StoreActualNodeExitThreshold(Guid timerId, byte lane, ushort threshold)
    {
        NaeTime.Persistence.EntityFramework.Models.NodeLaneConfiguration? laneConfiguration = await _dbContext.NodeLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new NaeTime.Persistence.EntityFramework.Models.NodeLaneConfiguration()
            {
                Id = timerId,
                Lane = lane,
                ActualExitThreshold = threshold
            };
            _dbContext.NodeLaneConfigurations.Add(laneConfiguration);
        }
        else
        {
            laneConfiguration.ActualExitThreshold = threshold;
        }
    }
    public async Task StoreTimerActualLaneRadioFrequency(Guid timerId, byte lane, byte? band, int frequencyInMhz)
    {
        NaeTime.Persistence.EntityFramework.Models.TimerLaneConfiguration? timerLaneStatus = await _dbContext.TimerLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.LaneId == lane);

        if (timerLaneStatus == null)
        {
            timerLaneStatus = new NaeTime.Persistence.EntityFramework.Models.TimerLaneConfiguration()
            {
                Id = timerId,
                ActualBandId = band,
                ActualFrequencyInMhz = frequencyInMhz
            };
            _dbContext.TimerLaneConfigurations.Add(timerLaneStatus);
        }
        else
        {
            timerLaneStatus.ActualBandId = band;
            timerLaneStatus.ActualFrequencyInMhz = frequencyInMhz;
        }
    }
    public async Task StoreTimerActualLaneStatus(Guid timerId, byte lane, bool isEnabled)
    {
        NaeTime.Persistence.EntityFramework.Models.TimerLaneConfiguration? timerLaneStatus = await _dbContext.TimerLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.LaneId == lane);

        if (timerLaneStatus == null)
        {
            timerLaneStatus = new NaeTime.Persistence.EntityFramework.Models.TimerLaneConfiguration()
            {
                Id = timerId,
                LaneId = lane,
                ActualIsEnabled = isEnabled
            };
            _dbContext.TimerLaneConfigurations.Add(timerLaneStatus);
        }
        else
        {
            timerLaneStatus.ActualIsEnabled = isEnabled;
        }
    }

    public async Task<IEnumerable<SerialEsp32Node>> GetAllSerialEsp32NodeTimers()
    {
        List<SerialEsp32Node> serialNodes = await _dbContext.SerialEsp32Nodes
            .Select(x => new SerialEsp32Node(x.Id, x.Name, x.Port))
            .ToListAsync().ConfigureAwait(false);

        return serialNodes;
    }

    public async Task<IEnumerable<TimerDetails>> GetAllTimerDetails()
    {
        List<TimerDetails> lapRF8Channels = await _dbContext.EthernetLapRF8Channels
            .Select(x => new TimerDetails(x.Id, x.Name, TimerType.EthernetLapRF8Channel))
            .ToListAsync().ConfigureAwait(false);

        lapRF8Channels.AddRange(await _dbContext.SerialEsp32Nodes
            .Select(x => new TimerDetails(x.Id, x.Name, TimerType.SerialEsp32Node))
            .ToListAsync().ConfigureAwait(false));

        //when there are more timer types will need to add them all to a larger list

        return lapRF8Channels;
    }
    public async Task<IEnumerable<TimerDetails>> GetTimerDetails(IEnumerable<Guid> timerIds)
    {
        List<TimerDetails> lapRF8Channels = await _dbContext.EthernetLapRF8Channels
                .Where(x => timerIds.Contains(x.Id))
                .Select(x => new TimerDetails(x.Id, x.Name, TimerType.EthernetLapRF8Channel))
                .ToListAsync().ConfigureAwait(false);
        lapRF8Channels.AddRange(await _dbContext.SerialEsp32Nodes
            .Where(x => timerIds.Contains(x.Id))
            .Select(x => new TimerDetails(x.Id, x.Name, TimerType.SerialEsp32Node))
            .ToListAsync().ConfigureAwait(false));
        return lapRF8Channels;
    }
    public async Task<TimerDetails?> GetTimerDetails(Guid timerId)
    {
        IEnumerable<TimerDetails> details = await GetTimerDetails([timerId]).ConfigureAwait(false);
        return details.FirstOrDefault();
    }
    public async Task<EthernetLapRF8ChannelTimer?> GetEthernetLapRF8ChannelTimer(Guid timerId)
    {
        var timer = await _dbContext.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == timerId).ConfigureAwait(false);

        return timer == null ? null : new EthernetLapRF8ChannelTimer(timer.Id, timer.Name, new IPAddress(timer.IpAddress), timer.Port);
    }
    public async Task<IEnumerable<EthernetLapRF8ChannelTimer>> GetAllEthernetLapRF8ChannelTimers()
    {
        List<EthernetLapRF8ChannelTimer> timerDetails = await _dbContext.EthernetLapRF8Channels
            .Select(x => new EthernetLapRF8ChannelTimer(x.Id, x.Name, new IPAddress(x.IpAddress), x.Port))
            .ToListAsync().ConfigureAwait(false);

        return timerDetails;
    }
    public async Task<SerialEsp32Node?> GetSerialEsp32NodeTimer(Guid timerId)
    {
        var timer = await _dbContext.SerialEsp32Nodes.FirstOrDefaultAsync(x => x.Id == timerId).ConfigureAwait(false);

        return timer == null ? null : new SerialEsp32Node(timer.Id, timer.Name, timer.Port);
    }
    public async Task<TimerType> GetTimerType(Guid timerId)
    {
        if (await _dbContext.EthernetLapRF8Channels.AnyAsync(x => x.Id == timerId))
        {
            return TimerType.EthernetLapRF8Channel;
        }

        if (await _dbContext.SerialEsp32Nodes.AnyAsync(x => x.Id == timerId))
        {
            return TimerType.SerialEsp32Node;
        }

        return TimerType.Unknown;
    }

    public async Task<IEnumerable<LaneConfiguration>> GetLaneConfigurations(IEnumerable<Guid> includedTimers)
    {
        Dictionary<byte, IGrouping<byte, NaeTime.Persistence.EntityFramework.Models.SystemLaneConfiguration>> systemLaneConfiguration = await _dbContext.SystemLaneConfigurations
            .GroupBy(x => x.LaneId)
            .ToDictionaryAsync(x => x.Key);

        Dictionary<byte, IGrouping<byte, NaeTime.Persistence.EntityFramework.Models.TimerLaneConfiguration>> timerLaneConfiguration = await _dbContext.TimerLaneConfigurations
            .Where(x => includedTimers.Contains(x.Id))
            .GroupBy(x => x.LaneId)
            .ToDictionaryAsync(x => x.Key);

        Dictionary<byte, IGrouping<byte, NaeTime.Persistence.EntityFramework.Models.NodeLaneConfiguration>> nodeLaneConfiguration = await _dbContext.NodeLaneConfigurations
            .Where(x => includedTimers.Contains(x.Id))
            .GroupBy(x => x.Lane)
            .ToDictionaryAsync(x => x.Key);

        Dictionary<byte, IGrouping<byte, NaeTime.Persistence.EntityFramework.Models.LapRFLaneConfiguration>> lapRFLaneConfiguration = await _dbContext.LapRFConfigurations
            .Where(x => includedTimers.Contains(x.Id))
            .GroupBy(x => x.Lane)
            .ToDictionaryAsync(x => x.Key);

        HashSet<byte> lanes = new(systemLaneConfiguration.Keys
            .Union(timerLaneConfiguration.Keys)
            .Union(nodeLaneConfiguration.Keys)
            .Union(lapRFLaneConfiguration.Keys));

        List<LaneConfiguration> laneConfigurations = new();
        foreach (byte lane in lanes)
        {
            systemLaneConfiguration.TryGetValue(lane, out IGrouping<byte, NaeTime.Persistence.EntityFramework.Models.SystemLaneConfiguration>? systemLanes);
            timerLaneConfiguration.TryGetValue(lane, out IGrouping<byte, NaeTime.Persistence.EntityFramework.Models.TimerLaneConfiguration>? timerLanes);
            nodeLaneConfiguration.TryGetValue(lane, out IGrouping<byte, NaeTime.Persistence.EntityFramework.Models.NodeLaneConfiguration>? nodeLanes);
            lapRFLaneConfiguration.TryGetValue(lane, out IGrouping<byte, NaeTime.Persistence.EntityFramework.Models.LapRFLaneConfiguration>? lapRFLanes);

            bool? isEnabled = systemLanes?.FirstOrDefault()?.IsEnabled;
            int? frequency = systemLanes?.FirstOrDefault()?.Frequency;
            byte? bandId = systemLanes?.FirstOrDefault()?.BandId;

            List<TimerLaneConfiguredField> fields = new();
            if (timerLanes != null)
            {
                foreach (NaeTime.Persistence.EntityFramework.Models.TimerLaneConfiguration timerLane in timerLanes)
                {
                    fields.Add(new TimerLaneConfiguredField(lane, timerLane.Id, timerLane.DesiredIsEnabled, timerLane.ActualIsEnabled, TimerLaneField.IsEnabled));
                    fields.Add(new TimerLaneConfiguredField(lane, timerLane.Id, timerLane.DesiredFrequencyInMhz, timerLane.ActualFrequencyInMhz, TimerLaneField.Frequency));
                    fields.Add(new TimerLaneConfiguredField(lane, timerLane.Id, timerLane.DesiredBandId, timerLane.ActualBandId, TimerLaneField.BandId));
                }
            }

            if (nodeLanes != null)
            {
                foreach (NaeTime.Persistence.EntityFramework.Models.NodeLaneConfiguration nodeLane in nodeLanes)
                {
                    fields.Add(new TimerLaneConfiguredField(lane, nodeLane.Id, nodeLane.DesiredEntryThreshold, nodeLane.ActualEntryThreshold, TimerLaneField.EntryThreshold));
                    fields.Add(new TimerLaneConfiguredField(lane, nodeLane.Id, nodeLane.DesiredExitThreshold, nodeLane.ActualExitThreshold, TimerLaneField.ExitThreshold));
                }
            }

            if (lapRFLanes != null)
            {
                foreach (NaeTime.Persistence.EntityFramework.Models.LapRFLaneConfiguration lapRFLane in lapRFLanes)
                {
                    fields.Add(new TimerLaneConfiguredField(lane, lapRFLane.Id, lapRFLane.DesiredGain, lapRFLane.ActualGain, TimerLaneField.Gain));
                    fields.Add(new TimerLaneConfiguredField(lane, lapRFLane.Id, lapRFLane.DesiredThreshold, lapRFLane.ActualThreshold, TimerLaneField.Threshold));
                }
            }

            laneConfigurations.Add(new LaneConfiguration(lane, isEnabled, bandId, frequency, fields));
        }

        return laneConfigurations;
    }
}
