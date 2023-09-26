using ImmersionRC.LapRF.Abstractions;

namespace ImmersionRC.LapRF.Communication;
internal class LapRFCommunicationFactory : ILapRFCommunicationFactory
{
    public Task<ILapRFCommunication> CreateCommunication(LapRFDeviceConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}
