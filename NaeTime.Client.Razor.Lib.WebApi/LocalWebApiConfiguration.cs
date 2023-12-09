using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Lib.WebApi;
public class LocalWebApiConfiguration : ApiConfigurationBase, ILocalApiConfiguration
{
    private const int _addressId = 0;

    public LocalWebApiConfiguration(ISimpleStorageProvider storageProvider) : base(storageProvider)
    {

    }

    public Task<string?> GetAddressAsync() => this.GetPropertyObjectValue<string>(_addressId);

    public override IEnumerable<(int key, string displayName, Type valueType)> GetProperties()
    {
        yield return (_addressId, "Address", typeof(string));
    }


    public override IEnumerable<(int key, string validationError)> ValidateProperties(IEnumerable<ApiConfigurationPropertyValue> values)
    {
        var addressProperty = values.SingleOrDefault(x => x.Id == _addressId);
        if (addressProperty == null || string.IsNullOrWhiteSpace(addressProperty.Value?.ToString()))
        {
            yield return (_addressId, "Address is required");
        }
    }
}
