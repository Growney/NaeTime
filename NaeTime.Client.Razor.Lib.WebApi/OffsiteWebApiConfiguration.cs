using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class OffSiteWebApiConfiguration : WebApiConfigurationBase, IOffSiteApiConfiguration
{
    public OffSiteWebApiConfiguration(ISimpleStorageProvider storageProvider) : base(storageProvider)
    {

    }

}
