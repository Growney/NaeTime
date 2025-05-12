
namespace ELRS.Backpack;

internal class BackpackConnectionFactory : IBackpackConnectionFactory
{
    private readonly IBackpackCommandSerializer _commandSerializer;

    public BackpackConnectionFactory(IBackpackCommandSerializer commandSerializer)
    {
        _commandSerializer = commandSerializer ?? throw new ArgumentNullException(nameof(commandSerializer));
    }

    public IBackpackConnection Create(string port) => new BackpackConnection(port, _commandSerializer);
}
