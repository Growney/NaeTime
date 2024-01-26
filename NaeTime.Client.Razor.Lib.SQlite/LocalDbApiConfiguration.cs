using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Lib.SQlite;
internal class LocalDbApiConfiguration : ApiConfigurationBase, ILocalApiConfiguration
{
    public LocalDbApiConfiguration(ISimpleStorageProvider storage) : base(storage)
    {
    }

    public override IEnumerable<(int key, string displayName, Type valueType)> GetProperties() => Enumerable.Empty<(int key, string displayName, Type valueType)>();

    public override IEnumerable<(int key, string validationError)> ValidateProperties(IEnumerable<ApiConfigurationPropertyValue> values) => Enumerable.Empty<(int key, string validationError)>();

}
