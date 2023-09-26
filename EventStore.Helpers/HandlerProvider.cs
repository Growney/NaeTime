namespace EventStore.Helpers;
internal class HandlerProvider : IHandlerProvider
{
    private readonly List<IHandler> _handlers = new();

    public HandlerProvider(List<IHandler> handlers)
    {
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
    }

    public async Task Start()
    {
        foreach (var handler in _handlers)
        {
            try
            {
                await handler.Start();
            }
            catch (Exception)
            {
                //todo log
            }
        }
    }

    public async Task Stop()
    {
        foreach (var handler in _handlers)
        {
            try
            {
                await handler.Stop();
            }
            catch (Exception)
            {
                //todo log
            }
        }
    }
}
