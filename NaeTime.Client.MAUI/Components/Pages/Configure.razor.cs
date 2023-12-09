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

    [Parameter]
    [SupplyParameterFromQuery]
    public bool Force { get; set; }

    private List<(ApiConfigurationProperty property, ApiConfigurationPropertyValue value)> _localValues = new();
    private Dictionary<int, string> _localValidationErrors = new();

    private List<(ApiConfigurationProperty property, ApiConfigurationPropertyValue value)> _offsiteValues = new();
    private Dictionary<int, string> _offsiteValidationErrors = new();

    private string? FormValidationErrors { get; set; } = null;
    private bool IsSkipConfigurationPageSet { get; set; }
    private bool IsLocalEnabled { get; set; }
    private bool IsOffSiteEnabled { get; set; }

    private async Task PopulateConfigurationValues(List<(ApiConfigurationProperty property, ApiConfigurationPropertyValue value)> valuesList, IApiConfiguration configuration)
    {
        valuesList.Clear();
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
    private Task SetSkipConfigurationPage(bool value) => SimpleStorage.SetAsync(c_skipConfigurationPageKey, value.ToString());
    private void MoveToNextPage()
    {
        NavigationManager.NavigateTo("/overview");
    }
    protected override async Task OnParametersSetAsync()
    {
        IsSkipConfigurationPageSet = await CheckIfSkipConfigurationPageIsSetAsync();
        IsLocalEnabled = await LocalConfiguration.IsEnabledAsync();
        IsOffSiteEnabled = await OffsiteConfiguration.IsEnabledAsync();

        if (!Force && IsSkipConfigurationPageSet)
        {
            bool eitherIsEnabled = IsLocalEnabled || IsOffSiteEnabled;
            bool isLocalValid = !IsLocalEnabled || await LocalConfiguration.IsCurrentConfigurationValidAsync();
            bool isOffSiteValid = !IsOffSiteEnabled || await OffsiteConfiguration.IsCurrentConfigurationValidAsync();

            if (eitherIsEnabled && isLocalValid && isOffSiteValid)
            {
                MoveToNextPage();
            }
        }

        await PopulateConfigurationValues(_localValues, LocalConfiguration);
        await PopulateConfigurationValues(_offsiteValues, OffsiteConfiguration);

        await base.OnParametersSetAsync();
    }

    public async Task OnClickConnect()
    {
        FormValidationErrors = null;

        if (!IsLocalEnabled && !IsOffSiteEnabled)
        {
            FormValidationErrors = "Offsite and/or Local configuration required";
            return;
        }

        bool isLocalValid = !IsLocalEnabled || IsValidConfiguration(LocalConfiguration, _localValues.Select(x => x.value), _localValidationErrors);
        bool isOffSiteValid = !IsOffSiteEnabled || IsValidConfiguration(OffsiteConfiguration, _offsiteValues.Select(x => x.value), _offsiteValidationErrors);
        if (!isLocalValid || !isOffSiteValid)
        {
            return;
        }

        await LocalConfiguration.SetEnabledAsync(IsLocalEnabled);
        await OffsiteConfiguration.SetEnabledAsync(IsOffSiteEnabled);
        await SetSkipConfigurationPage(IsSkipConfigurationPageSet);

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
