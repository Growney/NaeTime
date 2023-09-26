using Microsoft.Extensions.Logging;
using NaeTime.Node.Abstractions.Models;
using NaeTime.Node.Abstractions.Repositories;
using System.Text.Json;

namespace NaeTime.Node.FileStorage;

public class NodeFileConfigurationRepository : IConfigurationRepository
{
    private NodeConfiguration? _currentConfiguration;
    private const string c_configurationFileName = "config.json";


    private readonly ILogger<NodeFileConfigurationRepository> _logger;

    public NodeFileConfigurationRepository(ILogger<NodeFileConfigurationRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NodeConfiguration?> GetNodeConfiguration()
    {
        if (!File.Exists(c_configurationFileName))
        {
            _logger.LogWarning("Configuration file {configurationFileName} does not exist", c_configurationFileName);
            return null;
        }
        else
        {
            _logger.LogInformation("Configuration file found");
        }
        var fileText = await File.ReadAllTextAsync(c_configurationFileName);

        if (string.IsNullOrWhiteSpace(fileText))
        {
            _logger.LogWarning("Configuration file {configurationFileName} is empty", c_configurationFileName);
            return null;
        }

        try
        {
            var parsedConfig = JsonSerializer.Deserialize<NodeConfiguration>(fileText);

            return parsedConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing configuration file {configurationFileName}", c_configurationFileName);
            return null;
        }
    }


    public void SetConfiguration(NodeConfiguration configuration)
    {
        _currentConfiguration = configuration;
    }

    public async Task CommitConfiguration()
    {
        var configJson = JsonSerializer.Serialize(_currentConfiguration);

        await File.WriteAllTextAsync(c_configurationFileName, configJson);
    }
}
