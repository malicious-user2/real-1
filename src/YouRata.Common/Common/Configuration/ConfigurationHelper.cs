// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace YouRata.Common.Configuration;

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
        string settingsPath;
        string settingsDirectory;
        while (!searchingDirectory.GetFiles("*.sln").Any())
        {
            searchingDirectory = searchingDirectory.Parent ??
                                 throw new FileNotFoundException("Solution file could not be found in any parent directory");
        }

        if (searchingDirectory.Parent != null)
        {
            settingsPath = Path.Combine(searchingDirectory.Parent.FullName, YouRataConstants.SettingsFileName);
            settingsDirectory = searchingDirectory.Parent.FullName;
        }
        else
        {
            settingsPath = Path.Combine(startingDirectory.FullName, YouRataConstants.SettingsFileName);
            settingsDirectory = startingDirectory.FullName;
        }

        if (!Path.Exists(settingsPath))
        {
            CreateBlankConfig(settingsPath);
        }

        return new ConfigurationBuilder()
            .SetBasePath(settingsDirectory)
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
