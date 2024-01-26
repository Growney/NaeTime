using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Lib;
public abstract class ApiConfigurationBase : IApiConfiguration
{
    private readonly string _enabledKey;
    private readonly Dictionary<int, (string displayName, Type valueType)> _configuration = new();
    public IEnumerable<ApiConfigurationProperty> Properties => _configuration.Select(x => new ApiConfigurationProperty(x.Key, x.Value.displayName, x.Value.valueType));

    public event Action<int>? OnPropertyChanged;

    private readonly ISimpleStorageProvider _storage;
    public ApiConfigurationBase(ISimpleStorageProvider storage)
    {
        _storage = storage;

        foreach (var property in GetProperties())
        {
            _configuration.Add(property.key, (property.displayName, property.valueType));
        }

        _enabledKey = $"{GetType()}-Enabled";
    }

    public abstract IEnumerable<(int key, string displayName, Type valueType)> GetProperties();
    public abstract IEnumerable<(int key, string validationError)> ValidateProperties(IEnumerable<ApiConfigurationPropertyValue> values);

    public async IAsyncEnumerable<ApiConfigurationPropertyValue> GetPropertyValuesAsync(IEnumerable<int> ids)
    {
        foreach (int id in ids)
        {
            var propertyValue = await GetPropertyValueAsync(id);

            yield return new ApiConfigurationPropertyValue(id, propertyValue);
        }
    }
    public async Task SetPropertyValuesAsync(IEnumerable<ApiConfigurationPropertyValue> values)
    {
        foreach (var value in values)
        {
            await SetPropertyValue(value.Id, value.Value);
        }
    }
    private string? GetKey(int propertyId)
    {
        if (!_configuration.TryGetValue(propertyId, out var value))
        {
            return null;
        }
        return $"{this}-{value.displayName}-{propertyId}";
    }
    private Type? GetValueType(int propertyId)
    {
        if (!_configuration.TryGetValue(propertyId, out var value))
        {
            return null;
        }
        return value.valueType;
    }
    public async Task<object?> GetPropertyValueAsync(int id)
    {
        var key = GetKey(id);

        if (key == null)
        {
            return null;
        }

        var storedString = await _storage.GetAsync(key);

        if (storedString == null)
        {
            return null;
        }

        var valueType = GetValueType(id);

        if (valueType == null)
        {
            return null;
        }

        return ParseStoredString(valueType, storedString);
    }
    private object? ParseStoredString(Type valueType, string storedString)
        => valueType switch
        {
            Type stringType when stringType == typeof(string) => storedString,
            _ => throw new NotImplementedException()
        };
    public async Task SetPropertyValue(int id, object? value)
    {
        var key = GetKey(id);

        if (key == null)
        {
            return;
        }

        if (value == null)
        {
            await _storage.SetAsync(key, null);
            return;
        }

        var valueType = GetValueType(id);

        if (valueType == null)
        {
            return;
        }

        var objectString = ParseObject(valueType, value);

        await _storage.SetAsync(key, objectString);
        OnPropertyChanged?.Invoke(id);
    }

    private string? ParseObject(Type valueType, object value)
        => valueType switch
        {
            Type stringType when stringType == typeof(string) => value.ToString(),
            _ => throw new NotImplementedException()
        };

    public async Task<bool> IsCurrentConfigurationValidAsync()
    {
        var valueList = new List<ApiConfigurationPropertyValue>();
        await foreach (var currentValue in GetPropertyValuesAsync(Properties.Select(x => x.Id)))
        {
            valueList.Add(currentValue);
        }

        return !ValidateProperties(valueList).Any();

    }

    public async Task<bool> IsEnabledAsync()
    {
        var storedValue = await _storage.GetAsync(_enabledKey);
        if (!bool.TryParse(storedValue, out var isEnabled))
        {
            return false;
        }
        return isEnabled;
    }

    public Task SetEnabledAsync(bool isEnabled) => _storage.SetAsync(_enabledKey, isEnabled.ToString());

}
