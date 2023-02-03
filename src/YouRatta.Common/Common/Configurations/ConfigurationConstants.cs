using System;
using System.IO;

namespace YouRatta.Common.Configurations;

public static class ConfigurationConstants
{
    private readonly static string _settingsFile = "youratta-settings.json";
    private static readonly string _settingsPathInGitLab = Path
        .GetFullPath(Path
        .Combine(Directory.GetCurrentDirectory(), _settingsFile));
    private static readonly string _settingsPathInVS = Path
        .GetFullPath(Path
        .Combine(Directory.GetCurrentDirectory(), $"..\\..\\..\\{_settingsFile}"));

#if DEBUG
    public static string YouRattaSettingsPath => Path.Exists(_settingsPathInGitLab) ? _settingsPathInGitLab : _settingsPathInVS;
#else
    public static string YouRattaSettingsPath => _settingsPathInGitLab;
#endif

    public static string YouRattaSettingsFileName => _settingsFile;
}
