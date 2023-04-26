using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.CommandHandler.ASPNET.Client
{
    public class HandlerClientOptions
    {
        public string Path { get; set; } = Constants.HANDLE_PATH;
        public string MetadataHeader { get; set; } = Constants.METADATA_HEADER;
        public HandlerServiceOptions[] Services { get; set; } = new HandlerServiceOptions[0];
    }
}
