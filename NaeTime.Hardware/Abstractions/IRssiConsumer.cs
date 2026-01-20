using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Hardware.Abstractions;

public interface IRssiConsumer
{
    void HandleRssi(RssiValue value);
}
