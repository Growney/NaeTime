﻿namespace NaeTime.Node.WebAPI.Shared.Enumeration;
public enum SpiModeDto
{
    /// <summary>
    /// CPOL 0, CPHA 0. Polarity is idled low and data is sampled on rising edge of the clock signal.
    /// </summary>
    Mode0,

    /// <summary>
    /// CPOL 0, CPHA 1. Polarity is idled low and data is sampled on falling edge of the clock signal.
    /// </summary>
    Mode1,

    /// <summary>
    /// CPOL 1, CPHA 0. Polarity is idled high and data is sampled on falling edge of the clock signal.
    /// </summary>
    Mode2,

    /// <summary>
    /// CPOL 1, CPHA 1. Polarity is idled high and data is sampled on rising edge of the clock signal.
    /// </summary>
    Mode3
}
