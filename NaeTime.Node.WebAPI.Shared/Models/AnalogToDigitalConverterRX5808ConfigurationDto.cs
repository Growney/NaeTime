﻿namespace NaeTime.Node.WebAPI.Shared.Models;

public class AnalogToDigitalConverterRX5808ConfigurationDto
{
    public byte Id { get; set; }

    public byte DataPin { get; set; }
    public byte SelectPin { get; set; }
    public byte ClockPin { get; set; }

    public byte ADCId { get; set; }
    public byte ADCChannel { get; set; }

    public int Frequency { get; set; }
    public bool IsEnabled { get; set; }
}
