using KurrentDB.Client;

namespace EventDbLite.KurrentDb;
public class KurrentDbClientSettingsBuilder
{
    public IServiceProvider Services { get; }
    public KurrentDBClientSettings Settings { get; set; }
    public KurrentDbClientSettingsBuilder(IServiceProvider services, KurrentDBClientSettings initialSettings)
    {
        Services = services;
        Settings = initialSettings;
    }
}
