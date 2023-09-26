namespace NaeTime.Server.Abstractions.Hardware;
public interface IDetectorFactory
{
    IEnumerable<IDetector> CreateDetectors();
}
