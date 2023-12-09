namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface ISimpleStorageProvider
{
    Task SetAsync(string key, string? value);
    Task<string?> GetAsync(string key);
}
