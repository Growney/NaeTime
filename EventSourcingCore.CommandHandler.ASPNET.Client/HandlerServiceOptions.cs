using System;
using System.Collections.Generic;

namespace EventSourcingCore.CommandHandler.ASPNET.Client
{
    public class HandlerServiceOptions
    {
        public string Address { get; set; }
        public string Path { get; set; } = null;
        public string MetadataHeader { get; set; } = null;
        public string[] Identifiers { get; set; } = new string[0];
    }
}
