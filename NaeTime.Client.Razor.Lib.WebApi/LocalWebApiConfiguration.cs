using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class LocalWebApiConfiguration : WebApiConfigurationBase, ILocalApiConfiguration
{
    public LocalWebApiConfiguration(ISimpleStorageProvider storageProvider) : base(storageProvider)
    {

    }
}
