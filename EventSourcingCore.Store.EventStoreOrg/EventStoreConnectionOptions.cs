using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Store.EventStoreOrg
{
    public class EventStoreConnectionOptions
    {
        public string ConnectionString { get; set; }
        public bool AllowPersistentSubscriptionCreation { get; set; } = false;
        public string CreationUsername { get; set; }
        public string CreationPassword { get; set; }
    }
}
