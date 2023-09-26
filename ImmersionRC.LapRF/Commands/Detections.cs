namespace ImmersionRC.LapRF.Commands;
internal class Detections : CommandBase
{
    public Detections(byte pilotId, long realTimeClockTime, short statusFlag, int decoderId, int number, short peakHeight, short detectionFlags)
        : base(pilotId, realTimeClockTime, statusFlag)
    {
        DecoderId = decoderId;
        Number = number;
        PeakHeight = peakHeight;
        DetectionFlags = detectionFlags;
    }

    public int DecoderId { get; }
    public int Number { get; }
    public short PeakHeight { get; }
    public short DetectionFlags { get; }

}
