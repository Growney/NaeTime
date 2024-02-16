using NaeTime.Messages.Events.Hardware;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub;
using System.Net;

namespace NaeTime.Persistence;
public class HardwareService : ISubscriber
{
    private readonly IRepositoryFactory _repositoryFactory;

    public HardwareService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public async Task When(EthernetLapRF8ChannelConfigured configuredEvent)
    {
        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository().ConfigureAwait(false);

        var addressBytes = configuredEvent.IpAddress.GetAddressBytes();

        await hardwareRepository.AddOrUpdateEthernetLapRF8Channel(configuredEvent.TimerId, configuredEvent.Name, addressBytes, configuredEvent.Port).ConfigureAwait(false);
    }
    public async Task When(TimerConnectionEstablished connectionEstablishedEvent)
    {
        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository().ConfigureAwait(false);
        await hardwareRepository.SetTimerConnectionStatus(connectionEstablishedEvent.TimerId, true, connectionEstablishedEvent.UtcTime).ConfigureAwait(false);
    }
    public async Task When(TimerDisconnected disconnectedEvent)
    {
        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository().ConfigureAwait(false);
        await hardwareRepository.SetTimerConnectionStatus(disconnectedEvent.TimerId, false, disconnectedEvent.UtcTime).ConfigureAwait(false);
    }

    public async Task<TimerDetailsResponse> On(TimerDetailsRequest request)
    {
        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository().ConfigureAwait(false);

        var timerDetails = await hardwareRepository.GetAllTimerDetails().ConfigureAwait(false);

        return new TimerDetailsResponse(timerDetails.Select(x => new TimerDetailsResponse.TimerDetails(x.Id, x.Name, x.Type switch
        {
            Models.TimerType.EthernetLapRF8Channel => TimerDetailsResponse.TimerType.EthernetLapRF8Channel,
            _ => throw new NotImplementedException()
        })));

    }

    public async Task<EthernetLapRF8ChannelTimersResponse> On(EthernetLapRF8ChannelTimersRequest request)
    {
        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository().ConfigureAwait(false);

        var timerDetails = await hardwareRepository.GetAllEthernetLapRF8ChannelTimers().ConfigureAwait(false);

        return new EthernetLapRF8ChannelTimersResponse(
            timerDetails.Select(x => new EthernetLapRF8ChannelTimersResponse.EthernetLapRF8Channel(x.Id, new IPAddress(x.IpAddress), x.Port)));
    }

    public async Task<EthernetLapRF8ChannelResponse?> On(EthernetLapRF8ChannelRequest request)
    {
        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository().ConfigureAwait(false);

        var timer = await hardwareRepository.GetEthernetLapRF8Channel(request.TimerId).ConfigureAwait(false);

        return timer == null ? null : new EthernetLapRF8ChannelResponse(timer.Id, timer.Name, new IPAddress(timer.IpAddress), timer.Port);
    }
}
