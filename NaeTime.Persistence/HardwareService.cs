using NaeTime.Messages.Events.Hardware;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub;

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
        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository();

        var addressBytes = configuredEvent.IpAddress.GetAddressBytes();

        await hardwareRepository.AddOrUpdateEthernetLapRF8Channel(configuredEvent.TimerId, configuredEvent.Name, addressBytes, configuredEvent.Port);
    }
    public async Task When(TimerConnectionEstablished connectionEstablishedEvent)
    {
        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository();
        await hardwareRepository.SetTimerConnectionStatus(connectionEstablishedEvent.TimerId, true, connectionEstablishedEvent.UtcTime);
    }
    public async Task When(TimerDisconnected disconnectedEvent)
    {
        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository();
        await hardwareRepository.SetTimerConnectionStatus(disconnectedEvent.TimerId, false, disconnectedEvent.UtcTime);
    }

    public async Task<TimerDetailsResponse> On(TimerDetailsRequest request)
    {
        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository();

        var timerDetails = await hardwareRepository.GetAllTimerDetails();

        return new TimerDetailsResponse(timerDetails.Select(x => new TimerDetailsResponse.TimerDetails(x.Id, x.Name, x.Type switch
        {
            Models.TimerType.EthernetLapRF8Channel => TimerDetailsResponse.TimerType.EthernetLapRF8Channel,
            _ => throw new NotImplementedException()
        })));

    }


}
