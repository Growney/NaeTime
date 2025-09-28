using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeLane(byte Lane, Guid PilotId, bool IsEnabled, byte? BandId, int FrequencyInMHz);