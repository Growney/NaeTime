using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Abstractions.Models
{
    public class PilotNotificationConfiguration
    {
        public Guid PilotId { get; set; }
        public NotificationConfiguration Configuration { get; set; } = new();
    }
}
