using System;
using System.IO;

namespace YouRatta.Common;

public static class ConfigurationConstants
{
    private static readonly string _settingsPathInGitLab = Path
        .GetFullPath(Path
        .Combine(Directory.GetCurrentDirectory(), "youratta-settings.json"));
    private static readonly string _settingsPathInVS = Path
        .GetFullPath(Path
        .Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\..\\youratta-settings.json"));

#if DEBUG
    public static string YouRattaSettingsPath => Path.Exists(_settingsPathInGitLab) ? _settingsPathInGitLab : _settingsPathInVS;
#else
    public static string YouRattaSettingsPath => _settingsPathInGitLab;
#endif
}
