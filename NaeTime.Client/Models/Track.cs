namespace NaeTime.Client.Models;
public class Track
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public List<Detector> Detectors { get; set; } = [];
    public byte MaxLanes { get; set; }

    public bool CanMoveDetectorUp(Guid timerId)
    {
        Detector? detector = Detectors.FirstOrDefault(x => x.Id == timerId);

        if (detector == null)
        {
            return false;
        }

        int index = Detectors.IndexOf(detector);
        if (index is (-1) or 0)
        {
            return false;
        }

        return true;
    }
    public void MoveDetectorUp(Guid timerId)
    {
        Detector? detector = Detectors.FirstOrDefault(x => x.Id == timerId);

        if (detector == null)
        {
            return;
        }

        int index = Detectors.IndexOf(detector);
        if (index is (-1) or 0)
        {
            return;
        }

        Detectors.RemoveAt(index);
        Detectors.Insert(index - 1, detector);
    }
    public bool CanMoveDetectorDown(Guid timerId)
    {
        Detector? detector = Detectors.FirstOrDefault(x => x.Id == timerId);

        if (detector == null)
        {
            return false;
        }
        int index = Detectors.IndexOf(detector);
        if (index == -1 || index == Detectors.Count - 1)
        {
            return false;
        }

        return true;
    }
    public void MoveDetectorDown(Guid timerId)
    {
        Detector? detector = Detectors.FirstOrDefault(x => x.Id == timerId);

        if (detector == null)
        {
            return;
        }
        int index = Detectors.IndexOf(detector);
        if (index == -1 || index == Detectors.Count - 1)
        {
            return;
        }

        Detectors.RemoveAt(index);
        Detectors.Insert(index + 1, detector);
    }
}
