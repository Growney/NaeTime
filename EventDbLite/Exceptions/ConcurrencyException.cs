namespace EventDbLite.Exceptions;

public class ConcurrencyException(long expectedVersion, long actualVersion) : Exception($"Concurrency conflict: expected version {expectedVersion}, but actual version is {actualVersion}.")
{
    public long ExpectedVersion { get; } = expectedVersion;
    public long ActualVersion { get; } = actualVersion;
}
