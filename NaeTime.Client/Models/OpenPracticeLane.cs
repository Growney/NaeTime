using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Models;
public class OpenPracticeLane
{
    public byte Lane { get; set; }
    public Guid? PilotId { get; set; }
    public bool IsEnabled { get; set; }
    public byte? BandId { get; set; }
    public int FrequencyInMHz { get; set; }
}
