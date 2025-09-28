using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Models;
public class Session
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid TrackId { get; set; }
    public SessionType SessionType { get; set; }
}
