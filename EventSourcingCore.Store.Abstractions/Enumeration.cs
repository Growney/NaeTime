namespace EventSourcingCore.Store.Abstractions
{
    public enum StreamDirection
    {
        Forward,
        Reverse
    }

    public enum NakAction
    {
        //
        // Summary:
        //     Client unknown on action. Let server decide
        Unknown = 0,
        //
        // Summary:
        //     Park message do not resend. Put on poison queue
        Park = 1,
        //
        // Summary:
        //     Explicitly retry the message.
        Retry = 2,
        //
        // Summary:
        //     Skip this message do not resend do not put in poison queue
        Skip = 3,
        //
        // Summary:
        //     Stop the subscription.
        Stop = 4
    }
}
