using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace YouRatta.Common.Configurations;

public sealed class ConfigurationHelper
{
    private readonly string[] _args;
    public ConfigurationHelper(string[] args)
    {
        _args = args;
    }

    public IConfiguration Build()
    {
        if (!Path.Exists(ConfigurationConstants.YouRattaSettingsPath))
        {
            CreateBlankConfig();
        }
        return new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(ConfigurationConstants.YouRattaSettingsPath))
            .AddJsonFile(ConfigurationConstants.YouRattaSettingsFileName, false, true)
            .AddEnvironmentVariables()
            .AddCommandLine(_args)
            .Build();
    }

    public void CreateBlankConfig()
    {
        if (Path.Exists(ConfigurationConstants.YouRattaSettingsPath))
        {
            throw new IOException($"Configuration file {ConfigurationConstants.YouRattaSettingsPath} already exists");
        }
        Console.WriteLine(ConfigurationConstants.YouRattaSettingsPath);
        ConfigurationWriter writer = new ConfigurationWriter(ConfigurationConstants.YouRattaSettingsPath);
        writer.WriteBlankFile();
    }
}
