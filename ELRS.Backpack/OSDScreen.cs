using System.Collections.Concurrent;

namespace ELRS.Backpack;

internal record OSDElement(string Message, OSDPresentation Presentation, byte Row, byte Column, DateTime ExpiresAt);

internal class OSDScreen
{
    private readonly ConcurrentDictionary<(byte Row, byte Column), OSDElement> _elements = new();
    private volatile bool _isDirty;

    public bool IsDirty => _isDirty;

    public void SetElement(string message, OSDPresentation presentation, byte row, byte column, TimeSpan? duration)
    {
        var element = new OSDElement(message, presentation, row, column, duration.HasValue ? DateTime.UtcNow + duration.Value : DateTime.MaxValue);
        _elements[(row, column)] = element;
        _isDirty = true;
    }

    public bool RemoveExpiredElements()
    {
        bool removed = false;
        DateTime now = DateTime.UtcNow;
        foreach (var kvp in _elements)
        {
            if (kvp.Value.ExpiresAt <= now && _elements.TryRemove(kvp.Key, out _))
            {
                removed = true;
            }
        }
        if (removed)
        {
            _isDirty = true;
        }
        return removed;
    }

    public IReadOnlyCollection<OSDElement> GetActiveElements()
    {
        return _elements.Values.ToList();
    }

    public void ClearDirty() => _isDirty = false;

    public bool HasActiveElements => !_elements.IsEmpty;
}
