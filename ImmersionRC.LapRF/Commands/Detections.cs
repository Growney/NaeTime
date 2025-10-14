namespace ImmersionRC.LapRF.Commands;
internal class Detections(byte pilotId, long realTimeClockTime, short statusFlag, int decoderId, int number, short peakHeight, short detectionFlags) : CommandBase(pilotId, realTimeClockTime, statusFlag)
{
    public int DecoderId { get; } = decoderId;
    public int Number { get; } = number;
    public short PeakHeight { get; } = peakHeight;
    public short DetectionFlags { get; } = detectionFlags;

}
