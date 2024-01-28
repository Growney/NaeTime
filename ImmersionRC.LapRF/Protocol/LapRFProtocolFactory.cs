using ImmersionRC.LapRF.Abstractions;

namespace ImmersionRC.LapRF.Protocol;
public class LapRFProtocolFactory : ILapRFProtocolFactory
{
    public ILapRFProtocol Create(ILapRFCommunication communication)
    {
        var statusProtocol = new StatusProtocol();
        var passingRecordProtocol = new PassingRecordProtocol();
        var radioFrequencySetupProtocol = new RadioFrequencySetupProtocol(communication);

        var protocol = new LapRFProtocol(communication, statusProtocol, passingRecordProtocol, radioFrequencySetupProtocol);

        return protocol;
    }
}
