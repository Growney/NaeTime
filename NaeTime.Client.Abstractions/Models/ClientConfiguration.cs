using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Abstractions.Models
{
    public class ClientConfiguration
    {
        public NotificationsConfiguration Notifications { get; set; } = new ();
    }
}
