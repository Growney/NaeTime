using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Models;
public class Detector
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DetectorType Type { get; set; }
    public byte SupportedLanes { get; set; }
}