using NaeTime.Persistence.Client.Abstractions;

namespace NaeTime.Client.MAUI.Lib;
public class LocalStorageProvider : ISimpleStorageProvider
{
    public Task<string?> GetAsync(string key) => Task.FromResult(Preferences.Get(key, null));

    public Task SetAsync(string key, string? value)
    {
        Preferences.Set(key, value);
        return Task.CompletedTask;
    }
}
