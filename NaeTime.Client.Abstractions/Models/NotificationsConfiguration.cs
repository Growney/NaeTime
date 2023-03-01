using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Abstractions.Models
{
    public class NotificationsConfiguration
    {
        public NotificationConfiguration MyLapStarted { get; set; } = new() { AddNotification = true, PlaySound = false};
        public NotificationConfiguration MyLapCompleted{ get; set; } = new() { AddNotification = true, PlaySound = false };
        public NotificationConfiguration MySplitStarted { get; set; } = new() { AddNotification = true, PlaySound = false };
        public NotificationConfiguration MySplitCompleted{ get; set; } = new() { AddNotification = true, PlaySound = false };

        public NotificationConfiguration OtherPilotLapStarted { get; set; } = new() { AddNotification = false, PlaySound = false };
        public NotificationConfiguration OtherPilotLapCompleted { get; set; } = new() { AddNotification = true, PlaySound = false };
        public NotificationConfiguration OtherPilotSplitStarted { get; set; } = new() { AddNotification = false, PlaySound = false };
        public NotificationConfiguration OtherPilotSplitCompleted { get; set; } = new() { AddNotification = false, PlaySound = false };

        public List<PilotNotificationConfiguration> SpecificPilotLapStarted { get; set; } = new();
        public List<PilotNotificationConfiguration> SpecificPilotLapCompleted { get; set; } = new();
        public List<PilotNotificationConfiguration> SpecificPilotSplitStarted { get; set; } = new();
        public List<PilotNotificationConfiguration> SpecificPilotSplitCompleted { get; set; } = new();
    }
}
