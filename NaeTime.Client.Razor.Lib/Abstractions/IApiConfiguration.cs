using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface IApiConfiguration
{
    public IEnumerable<ApiConfigurationProperty> Properties { get; }
    public IEnumerable<(int key, string validationError)> ValidateProperties(IEnumerable<ApiConfigurationPropertyValue> value);
    public IAsyncEnumerable<ApiConfigurationPropertyValue> GetPropertyValuesAsync(IEnumerable<int> keys);
    public Task<object?> GetPropertyValueAsync(int key);
    public Task SetPropertyValuesAsync(IEnumerable<ApiConfigurationPropertyValue> values);
    public Task<bool> IsCurrentConfigurationValid();
}
