namespace ImmersionRC.LapRF.Abstractions;
internal interface ILapRFCommunicationFactory
{
    Task<ILapRFCommunication> CreateCommunication(LapRFDeviceConfiguration configuration);
}
