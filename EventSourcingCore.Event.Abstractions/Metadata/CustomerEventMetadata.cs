using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Event.Abstractions.Metadata
{
    public class CustomerEventMetadata : IEventMetadata
    {
        public CustomerEventMetadata(Guid iD, string identifier, ZonedDateTime createdAt, ZonedDateTime validFrom, Guid userID, Guid customerID)
        {
            ID = iD;
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            CreatedAt = createdAt;
            ValidFrom = validFrom;
            UserID = userID;
            CustomerID = customerID;
        }

        public Guid ID { get; }
        public string Identifier { get; }
        public ZonedDateTime CreatedAt { get; }
        public ZonedDateTime ValidFrom { get; }
        public Guid UserID { get; }
        public Guid CustomerID { get; }
    }
}
