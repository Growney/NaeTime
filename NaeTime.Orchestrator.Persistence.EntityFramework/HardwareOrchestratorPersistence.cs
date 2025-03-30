using Microsoft.EntityFrameworkCore;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.EntityFramework;
using NaeTime.Persistence.EntityFramework.Models;
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
        LapRFLaneConfiguration? laneConfiguration = await _dbContext.LapRFConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new LapRFLaneConfiguration()
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
        LapRFLaneConfiguration? laneConfiguration = await _dbContext.LapRFConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new LapRFLaneConfiguration()
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
        NodeLaneConfiguration? laneConfiguration = await _dbContext.NodeLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new NodeLaneConfiguration()
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
        NodeLaneConfiguration? laneConfiguration = await _dbContext.NodeLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new NodeLaneConfiguration()
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
        TimerLaneConfiguration? timerLaneStatus = await _dbContext.TimerLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.LaneId == lane);
        if (timerLaneStatus == null)
        {
            timerLaneStatus = new TimerLaneConfiguration()
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
        TimerLaneConfiguration? timerLaneStatus = await _dbContext.TimerLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.LaneId == lane);

        if (timerLaneStatus == null)
        {
            timerLaneStatus = new TimerLaneConfiguration()
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
        EthernetLapRF8Channel? timer = await _dbContext.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == id);
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
        SerialEsp32Node? timer = await _dbContext.SerialEsp32Nodes.FirstOrDefaultAsync(x => x.Id == id);
        if (timer == null)
        {
            return false;
        }

        timer.Name = name;
        timer.Port = comPort;
        return true;
    }
    public async Task ConfigureSystemDesiredLaneRadioFrequency(byte lane, int frequencyInMhz)
    {
        SystemLaneConfiguration? laneConfiguration = await _dbContext.SystemLaneConfigurations.FirstOrDefaultAsync(x => x.LaneId == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new SystemLaneConfiguration()
            {
                Id = Guid.NewGuid(),
                LaneId = lane,
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
        SystemLaneConfiguration? laneConfiguration = await _dbContext.SystemLaneConfigurations.FirstOrDefaultAsync(x => x.LaneId == lane);

        if (laneConfiguration == null)
        {
            laneConfiguration = new SystemLaneConfiguration()
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
        _dbContext.TimerStatuses.Add(new TimerStatus()
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
        EthernetLapRF8Channel timer = new()
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
        SerialEsp32Node timer = new()
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
        _dbContext.TimerStatuses.Add(new TimerStatus()
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
        LapRFLaneConfiguration? laneConfiguration = await _dbContext.LapRFConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new LapRFLaneConfiguration()
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
        LapRFLaneConfiguration? laneConfiguration = await _dbContext.LapRFConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new LapRFLaneConfiguration()
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
        NodeLaneConfiguration? laneConfiguration = await _dbContext.NodeLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new NodeLaneConfiguration()
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
        NodeLaneConfiguration? laneConfiguration = await _dbContext.NodeLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.Lane == lane);
        if (laneConfiguration == null)
        {
            laneConfiguration = new NodeLaneConfiguration()
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
        TimerLaneConfiguration? timerLaneStatus = await _dbContext.TimerLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.LaneId == lane);

        if (timerLaneStatus == null)
        {
            timerLaneStatus = new TimerLaneConfiguration()
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
        TimerLaneConfiguration? timerLaneStatus = await _dbContext.TimerLaneConfigurations.FirstOrDefaultAsync(x => x.Id == timerId && x.LaneId == lane);

        if (timerLaneStatus == null)
        {
            timerLaneStatus = new TimerLaneConfiguration()
            {
                Id = timerId,
                ActualIsEnabled = isEnabled
            };
            _dbContext.TimerLaneConfigurations.Add(timerLaneStatus);
        }
        else
        {
            timerLaneStatus.ActualIsEnabled = isEnabled;
        }
    }
}
