using System;
using System.IO;
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
        return new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(ConfigurationConstants.YouRattaSettingsPath) ?? Directory.GetCurrentDirectory())
            .AddJsonFile(ConfigurationConstants.YouRattaSettingsFileName, false, true)
            .AddEnvironmentVariables()
            .AddCommandLine(_args)
            .Build();
    }
}
