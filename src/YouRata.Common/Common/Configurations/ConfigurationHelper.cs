using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace YouRata.Common.Configurations;

public sealed class ConfigurationHelper
{
    private readonly string[] _args;
    private readonly string _settingsFileName = "yourata-settings.json";
    private string _settingsPath;

    public ConfigurationHelper(string[] args)
    {
        _args = args;
        _settingsPath = string.Empty;
    }

    public IConfiguration Build()
    {
        DirectoryInfo startingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        DirectoryInfo searchingDirectory = startingDirectory;
        while (!searchingDirectory.GetFiles("*.sln").Any())
        {
            searchingDirectory = searchingDirectory.Parent ?? throw new FileNotFoundException("Solution file could not be found in any parent directory"); ;
        }
        if (searchingDirectory?.Parent != null)
        {
            _settingsPath = Path.Combine(searchingDirectory.Parent.FullName, _settingsFileName);
        }
        else
        {
            _settingsPath = Path.Combine(startingDirectory.FullName, _settingsFileName);
        }
        if (!Path.Exists(_settingsPath))
        {
            CreateBlankConfig();
        }
        return new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(_settingsPath))
            .AddJsonFile(_settingsFileName, false, true)
            .AddCommandLine(_args)
            .Build();
    }

    public void CreateBlankConfig()
    {
        if (Path.Exists(_settingsPath))
        {
            throw new IOException($"Configuration file {_settingsPath} already exists");
        }
        ConfigurationWriter writer = new ConfigurationWriter(_settingsPath);
        writer.WriteBlankFile();
    }

    public string SettingsFilePath => _settingsPath;
}
