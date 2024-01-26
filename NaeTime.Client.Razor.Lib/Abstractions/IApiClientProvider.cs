namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface IApiClientProvider
{
    public Task<bool> TryConnectionAsync(CancellationToken token);
    public Task<bool> IsValidAsync(CancellationToken token);
    public Task<bool> IsEnabledAsync(CancellationToken token);
}
