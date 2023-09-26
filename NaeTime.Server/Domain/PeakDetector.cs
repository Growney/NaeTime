namespace NaeTime.Server.Domain;

public enum PeakState
{
    None,
    PeakEntry,
    InsidePeak,
    PeakExit
}
public class PeakDetector
{
    private long _noiseThreshold;
    private long _currentPeakStart;
    private long _maxNoiseThreshold;
    private long _noiseThresholdFactor;
    private PeakState _peakState;

    public PeakDetector()
    {
        _noiseThreshold = 0;
        _currentPeakStart = -1;
        _maxNoiseThreshold = long.MinValue;
        _noiseThresholdFactor = 2;
        _peakState = PeakState.None;
    }

    public PeakState ProcessNext(long nextElement)
    {
        if (_peakState == PeakState.None)
        {
            if (nextElement >= _noiseThreshold)
            {
                _currentPeakStart = nextElement;
                _peakState = PeakState.PeakEntry;
            }
        }
        else if (_peakState == PeakState.PeakEntry)
        {
            if (nextElement >= _currentPeakStart)
            {
                _currentPeakStart = nextElement;
                _peakState = PeakState.InsidePeak;
            }
            else
            {
                if (_currentPeakStart > _maxNoiseThreshold)
                {
                    _maxNoiseThreshold = _currentPeakStart;
                }
                _currentPeakStart = -1;
                _peakState = PeakState.PeakExit;
            }
        }
        else if (_peakState == PeakState.InsidePeak)
        {
            if (nextElement >= _currentPeakStart)
            {
                _currentPeakStart = nextElement;
            }
            else
            {
                _currentPeakStart = nextElement;
                _peakState = PeakState.PeakExit;
            }
        }
        else if (_peakState == PeakState.PeakExit)
        {
            if (nextElement >= _noiseThreshold)
            {
                _currentPeakStart = nextElement;
                _peakState = PeakState.PeakEntry;
            }
            else
            {
                _peakState = PeakState.None;
            }
        }

        if (nextElement > _maxNoiseThreshold)
        {
            _noiseThreshold = nextElement * _noiseThresholdFactor;
        }

        return _peakState;
    }
}
