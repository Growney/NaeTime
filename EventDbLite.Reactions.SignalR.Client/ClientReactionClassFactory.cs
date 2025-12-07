using EventDbLite.Abstractions;
using EventDbLite.Reactions.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EventDbLite.Reactions.SignalR.Client;
internal class ClientReactionClassFactory : IReactionClassFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ClientReactionClassFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    public IReactionClassContainer<T> Create<T>()
        where T : class
    {
        T instance = ActivatorUtilities.CreateInstance<T>(_serviceProvider);

        if (instance == null)
        {
            throw new InvalidOperationException();
        }

        return ActivatorUtilities.CreateInstance<ClientReactionClass<T>>(_serviceProvider, instance);
    }
}
