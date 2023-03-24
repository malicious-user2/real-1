using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace YouRata.Common.Configurations;

public sealed class ConfigurationHelper
{
    private readonly string[] _args;

    public ConfigurationHelper(string[] args)
    {
        _args = args;
    }

    public IConfiguration Build()
    {
        DirectoryInfo startingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        DirectoryInfo searchingDirectory = startingDirectory;
        string _settingsPath = string.Empty;
        string _settingsDirectory = string.Empty;
        while (!searchingDirectory.GetFiles("*.sln").Any())
        {
            searchingDirectory = searchingDirectory.Parent ?? throw new FileNotFoundException("Solution file could not be found in any parent directory"); ;
        }
        if (searchingDirectory?.Parent != null)
        {
            _settingsPath = Path.Combine(searchingDirectory.Parent.FullName, YouRataConstants.SettingsFileName);
            _settingsDirectory = searchingDirectory.Parent.FullName;
        }
        else
        {
            _settingsPath = Path.Combine(startingDirectory.FullName, YouRataConstants.SettingsFileName);
            _settingsDirectory = startingDirectory.FullName;
        }
        if (!Path.Exists(_settingsPath))
        {
            CreateBlankConfig(_settingsPath);
        }
        return new ConfigurationBuilder()
            .SetBasePath(_settingsDirectory)
            .AddJsonFile(YouRataConstants.SettingsFileName, false, true)
            .AddCommandLine(_args)
            .Build();
    }

    private void CreateBlankConfig(string settingsPath)
    {
        if (Path.Exists(settingsPath))
        {
            throw new IOException($"Configuration file {settingsPath} already exists");
        }
        ConfigurationWriter writer = new ConfigurationWriter(settingsPath);
        writer.WriteBlankFile();
    }
}
