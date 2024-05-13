using ImmersionRC.LapRF.Abstractions;

namespace ImmersionRC.LapRF.Protocol;
public class LapRFProtocolFactory : ILapRFProtocolFactory
{
    public ILapRFProtocol Create(ILapRFCommunication communication)
    {
        StatusProtocol statusProtocol = new();
        PassingRecordProtocol passingRecordProtocol = new();
        RadioFrequencySetupProtocol radioFrequencySetupProtocol = new(communication);

        LapRFProtocol protocol = new(communication, statusProtocol, passingRecordProtocol, radioFrequencySetupProtocol);

        return protocol;
    }
}
