using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class OffSiteWebApiClientProvider : WebApiClientProviderBase, IOffSiteApiClientProvider
{
    private readonly OffSiteWebApiConfiguration _configuration;

    public OffSiteWebApiClientProvider(OffSiteWebApiConfiguration configuration) : base(configuration)
    {
        _configuration = configuration;
    }
}
