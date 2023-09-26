namespace NaeTime.Node.Domain;

public class AveragedRssiReading
{
    private long? currentTick = null;
    private int valueTotal = 0;
    private int valueCount = 0;
    private int? currentFrequency = 0;

    public bool IsNewAverage(long tick, int value, int frequency, out int previousAverage)
    {
        bool isNew = false;
        if (currentTick != null)
        {
            if (currentTick != tick)
            {
                isNew = true;
            }
        }
        if (currentFrequency != null)
        {
            if (currentFrequency != frequency)
            {
                isNew = true;
            }
        }

        valueTotal += value;
        valueCount++;
        currentTick = tick;
        currentFrequency = frequency;
        if (isNew)
        {
            previousAverage = valueTotal / valueCount;
            valueCount = 0;
            valueTotal = 0;
        }
        else
        {
            previousAverage = 0;
        }

        return isNew;
    }
}
