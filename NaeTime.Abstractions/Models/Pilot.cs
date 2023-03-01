using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Models
{
    public class Pilot
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? PhoneticName { get; set; }
    }
}
