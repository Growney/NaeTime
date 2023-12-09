using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Lib.WebApi.Abstractions;
public interface IWebApiConfiguration : IApiConfiguration
{
    public Task<string?> GetAddressAsync();
}
