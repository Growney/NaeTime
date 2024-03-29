﻿using NaeTime.Hardware.Abstractions;
using System.Diagnostics;

namespace NaeTime.Hardware;
public class SoftwareTimer : IDisposable, ISoftwareTimer
{
    private readonly Stopwatch _stopwatch;

    public SoftwareTimer()
    {
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

    public void Dispose() => _stopwatch.Stop();
}
