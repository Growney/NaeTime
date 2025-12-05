namespace EventDbLite.Abstractions;

public static class ExpectedVersion
{
    //
    // Summary:
    //     The write should not conflict with anything and should always succeed.
    public const long Any = -2;
    //
    // Summary:
    //     The stream should not yet exist. If it does exist treat that as a concurrency
    //     problem.
    public const long NoStream = -1;
    //
    // Summary:
    //     The stream should exist. If it or a metadata stream does not exist treat that
    //     as a concurrency problem.
    public const long StreamExists = -3;

    public const long End = long.MaxValue;

    public const long Beginning = 0;
}