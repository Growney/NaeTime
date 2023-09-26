using ImmersionRC.LapRF.Protocol;

namespace ImmersionRC.LapRF;
public interface ILapRFProtocol
{
    IStatusProtocol StatusProtocol { get; }
    IPassingRecordProtocol PassingRecordProtocol { get; }
    IRadioFrequencySetupProtocol RadioFrequencySetupProtocol { get; }


}
