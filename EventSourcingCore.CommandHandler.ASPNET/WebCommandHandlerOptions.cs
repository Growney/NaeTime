namespace EventSourcingCore.CommandHandler.ASPNET
{
    public class WebCommandHandlerOptions
    {
        public string HandlePath { get; set; } = "Handle";
        public string MetadataHeader { get; set; } = "Tensor-Command-Metadata";
    }
}
