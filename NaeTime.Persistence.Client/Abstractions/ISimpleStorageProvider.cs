namespace NaeTime.Persistence.Client.Abstractions;
public interface ISimpleStorageProvider
{
    Task SetAsync(string key, string? value);
    Task<string?> GetAsync(string key);
}
