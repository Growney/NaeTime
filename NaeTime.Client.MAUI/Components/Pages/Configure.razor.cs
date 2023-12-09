using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.MAUI.Components.Pages;
public partial class Configure : ComponentBase
{
    private const string c_skipConfigurationPageKey = "Skip-Configuration-Page";
    [Inject]
    public ILocalApiClientProvider LocalApiClientProvider { get; set; } = null!;
    [Inject]
    public ILocalApiConfiguration LocalConfiguration { get; set; } = null!;
    [Inject]
    public IOffSiteApiConfiguration OffsiteConfiguration { get; set; } = null!;
    [Inject]
    public ISimpleStorageProvider SimpleStorage { get; set; } = null!;
    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    private List<(ApiConfigurationProperty property, ApiConfigurationPropertyValue value)> _localValues = new();
    private Dictionary<int, string> _localValidationErrors = new();

    private List<(ApiConfigurationProperty property, ApiConfigurationPropertyValue value)> _offsiteValues = new();
    private Dictionary<int, string> _offsiteValidationErrors = new();

    private bool IsSkipConfigurationPageSet { get; set; }

    private async Task PopulateConfigurationValues(List<(ApiConfigurationProperty property, ApiConfigurationPropertyValue value)> valuesList, IApiConfiguration configuration)
    {
        var properties = configuration.Properties;

        var values = configuration.GetPropertyValuesAsync(properties.Select(x => x.Id));

        await foreach (var value in values)
        {
            var property = properties.FirstOrDefault(x => x.Id == value.Id);
            if (property != null)
            {
                valuesList.Add((property, value));
            }
        }
    }
    private async Task<bool> CheckIfSkipConfigurationPageIsSetAsync()
    {
        var storedValue = await SimpleStorage.GetAsync(c_skipConfigurationPageKey);
        if (!bool.TryParse(storedValue, out var isStoredValueSet))
        {
            return false;
        }
        return isStoredValueSet;
    }
    private void MoveToNextPage()
    {
        NavigationManager.NavigateTo("/overview");
    }
    protected override async Task OnInitializedAsync()
    {
        if (await CheckIfSkipConfigurationPageIsSetAsync())
        {
            bool isLocalValid = await LocalConfiguration.IsCurrentConfigurationValid();
            bool isOffSiteValid = await OffsiteConfiguration.IsCurrentConfigurationValid();

            if (isLocalValid && isOffSiteValid)
            {
                MoveToNextPage();
            }
        }

        await PopulateConfigurationValues(_localValues, LocalConfiguration);
        await PopulateConfigurationValues(_offsiteValues, OffsiteConfiguration);

        if (!_localValues.Any() && !_offsiteValues.Any())
        {
            MoveToNextPage();
        }
        await base.OnInitializedAsync();
    }

    public async Task OnClickConnect()
    {
        bool isLocalValid = IsValidConfiguration(LocalConfiguration, _localValues.Select(x => x.value), _localValidationErrors);
        bool isOffSiteValid = IsValidConfiguration(OffsiteConfiguration, _offsiteValues.Select(x => x.value), _offsiteValidationErrors);
        if (!isLocalValid || !isOffSiteValid)
        {
            return;
        }

        await LocalConfiguration.SetPropertyValuesAsync(_localValues.Select(x => x.value));
        await OffsiteConfiguration.SetPropertyValuesAsync(_offsiteValues.Select(x => x.value));

        MoveToNextPage();

    }
    private bool IsValidConfiguration(IApiConfiguration configuration, IEnumerable<ApiConfigurationPropertyValue> values, Dictionary<int, string> validationResults)
    {
        validationResults.Clear();

        var localErrors = configuration.ValidateProperties(values);

        foreach (var error in localErrors)
        {
            validationResults.Add(error.key, error.validationError);
        }

        return !validationResults.Any();
    }
}
