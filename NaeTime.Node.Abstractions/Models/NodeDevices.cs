using NaeTime.Node.Abstractions.Domain;

namespace NaeTime.Node.Abstractions.Models;

public class NodeDevices : IDisposable
{
    public IEnumerable<ITunedRssiDevice> TunedRssiDevices { get; }
    private readonly IEnumerable<IDisposable> _disposables;

    public NodeDevices(IEnumerable<ITunedRssiDevice> tunedRssiDevices, IEnumerable<IDisposable> disposables)
    {
        TunedRssiDevices = tunedRssiDevices ?? throw new ArgumentNullException(nameof(tunedRssiDevices));
        _disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}
