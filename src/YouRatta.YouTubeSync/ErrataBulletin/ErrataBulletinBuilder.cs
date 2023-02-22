using System;
using System.Collections.Generic;
using YouRatta.Common.Configurations;

namespace YouRatta.YouTubeSync.ErrataBulletin;

internal class ErrataBulletinBuilder
{
    private readonly ErrataBulletinConfiguration _configuration;

    private const string mdTemplateTitle = "# {0}";
    private readonly Dictionary<int, string> _keyVariablePairs;

    public ErrataBulletinBuilder(ErrataBulletinConfiguration config)
    {
        _configuration = config;
        _keyVariablePairs = new Dictionary<int, string>();
    }

    private string AddTitle(out string bulletinTemplate, out int variableNumber)
    {

    }

    public string Build()
    {
        int variableNumber = 0;
        if (string.IsNullOrEmpty(SnippetTitle)) throw new Exception("No title specified to build errata bulletin");
    }

    public string SnippetTitle;
}
