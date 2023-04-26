using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Store.EventStoreOrg.GRPC
{
    public class EventStoreConnectionOptions
    {
        public string ConnectionName { get; set; }
        public string Address { get; set; }
        public bool AllowPersistentSubscriptionCreation { get; set; } = false;
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
