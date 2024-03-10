namespace NaeTime.Client.Configuration.Abstractions;
public interface ISimpleStorageProvider
{
    Task SetAsync(string key, string? value);
    Task<string?> GetAsync(string key);
}
