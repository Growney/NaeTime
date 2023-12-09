namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface IApiClientProvider
{
    public DateTime? LastCommunication { get; }
    public Task<bool> TryConnectionAsync(CancellationToken token);
    public Task<bool> IsConfiguredAsync(CancellationToken token);
}
