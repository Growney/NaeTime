namespace EventDbLite.Abstractions;

public struct StreamPosition
{
    public static readonly StreamPosition Any = new(ExpectedVersion.Any);
    public static readonly StreamPosition NoStream = new(ExpectedVersion.NoStream);
    public static readonly StreamPosition StreamExists = new(ExpectedVersion.StreamExists);
    public static readonly StreamPosition Beginning = new(ExpectedVersion.Beginning);
    public static readonly StreamPosition End = new(ExpectedVersion.End);
    public static StreamPosition WithGlobalVersion(long expectedVersion) => new(expectedVersion);
    public static StreamPosition WithVersion(long expectedVersion) => new(expectedVersion, false);

    public long Version { get; }
    public bool IsGlobal { get; }

    private StreamPosition(long state, bool isGlobal = true)
    {
        Version = state;
        IsGlobal = isGlobal;
    }

    public static bool operator ==(StreamPosition left, StreamPosition right) => left.Version == right.Version;
    public static bool operator !=(StreamPosition left, StreamPosition right) => left.Version != right.Version;
    public static bool operator !=(long left, StreamPosition right) => left != right.Version;
    public static bool operator ==(long left, StreamPosition right) => left == right.Version;
    public static bool operator !=(StreamPosition left, long right) => left.Version != right;
    public static bool operator ==(StreamPosition left, long right) => left.Version == right;
    public override bool Equals(object? obj) => obj is StreamPosition state ? Version == state.Version : base.Equals(obj);
    public override int GetHashCode() => Version.GetHashCode();
    public bool IsValidUpdateVersion(long currentPosition) => Version switch
    {
        ExpectedVersion.Any or ExpectedVersion.NoStream or ExpectedVersion.StreamExists => true,
        _ => currentPosition == Version
    };
    public static implicit operator long(StreamPosition state) => state.Version;
    public static implicit operator StreamPosition(long version) => WithGlobalVersion(version);
}
