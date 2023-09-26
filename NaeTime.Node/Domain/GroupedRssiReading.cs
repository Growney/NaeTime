using NaeTime.Node.Abstractions.Models;

namespace NaeTime.Node.Domain;

public class GroupedRssiReading
{
    private readonly long _groupCompleteDistance;
    private readonly RssiReading[] _readings;

    private int? _currentFrequency = null;
    private int _currentIndex = 0;

    public GroupedRssiReading(long groupCompleteDistance)
    {
        _groupCompleteDistance = groupCompleteDistance;
        _readings = new RssiReading[groupCompleteDistance];
    }

    public bool IsNewGroup(RssiReading nextReading, out IEnumerable<RssiReading> readings)
    {
        _readings[_currentIndex] = nextReading;

        _currentIndex = (_currentIndex + 1) % _readings.Length;

        if (_currentFrequency == null)
        {
            _currentFrequency = nextReading.Frequency;
        }

        if (_currentFrequency != nextReading.Frequency)
        {
            readings = _readings.Take(_currentIndex + 1);
            _currentIndex = 0;
            return true;
        }

        if (_currentIndex == 0)
        {
            readings = _readings;
            return true;
        }
        else
        {
            readings = Enumerable.Empty<RssiReading>();
            return false;
        }

    }
}
