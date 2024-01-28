namespace ImmersionRC.LapRF.Abstractions;
public interface ILapRFProtocol
{
    IStatusProtocol StatusProtocol { get; }
    IPassingRecordProtocol PassingRecordProtocol { get; }
    IRadioFrequencySetupProtocol RadioFrequencySetupProtocol { get; }

    Task RunAsync(CancellationToken token);

}
