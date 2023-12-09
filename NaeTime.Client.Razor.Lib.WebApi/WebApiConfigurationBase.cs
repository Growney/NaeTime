using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.WebApi.Abstractions;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class WebApiConfigurationBase : ApiConfigurationBase, IWebApiConfiguration
{
    private const int _addressId = 0;

    public WebApiConfigurationBase(ISimpleStorageProvider storageProvider) : base(storageProvider)
    {

    }

    public Task<string?> GetAddressAsync() => this.GetPropertyObjectValue<string>(_addressId);

    public override IEnumerable<(int key, string displayName, Type valueType)> GetProperties()
    {
        yield return (_addressId, "Address", typeof(string));
        foreach (var property in GetAdditionalProperties())
        {
            yield return property;
        }
    }

    protected virtual IEnumerable<(int key, string displayName, Type valueType)> GetAdditionalProperties()
    {
        yield break;
    }

    public override IEnumerable<(int key, string validationError)> ValidateProperties(IEnumerable<ApiConfigurationPropertyValue> values)
    {
        var addressProperty = values.SingleOrDefault(x => x.Id == _addressId);
        if (addressProperty == null || string.IsNullOrWhiteSpace(addressProperty.Value?.ToString()))
        {
            yield return (_addressId, "Address is required");
        }
        foreach (var validationError in OnValidateProperties(values))
        {
            yield return validationError;
        }
    }

    protected virtual IEnumerable<(int key, string validationError)> OnValidateProperties(IEnumerable<ApiConfigurationPropertyValue> values)
    {
        yield break;
    }
}
