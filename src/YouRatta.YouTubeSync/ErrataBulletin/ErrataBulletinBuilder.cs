using System;
using System.Collections.Generic;
using System.Globalization;
using YouRatta.Common.Configurations;

namespace YouRatta.YouTubeSync.ErrataBulletin;

internal class ErrataBulletinBuilder
{
    private readonly ErrataBulletinConfiguration _configuration;

    private const string mdTemplateTitleTop1 = "# {0}";
    private const string mdTemplateTitleBottom1 = "#";
    private const string mdTemplateTitleBottom2 = "{0}";
    private const string mdTemplateTitleBottom3 = "===";

    public ErrataBulletinBuilder(ErrataBulletinConfiguration config)
    {
        _configuration = config;
    }

    private void AddTitle(ref string bulletinTemplate)
    {
        switch (_configuration.VideoTitleLocation)
        {
            case "top":
                bulletinTemplate = string.Format(CultureInfo.InvariantCulture, mdTemplateTitleTop1, SnippetTitle);
                bulletinTemplate += Environment.NewLine;
                bulletinTemplate += Environment.NewLine;
                break;
            case "bottom":
                bulletinTemplate += mdTemplateTitleBottom1;
                bulletinTemplate += Environment.NewLine;
                bulletinTemplate += string.Format(CultureInfo.InvariantCulture, mdTemplateTitleBottom2, SnippetTitle);
                bulletinTemplate += Environment.NewLine;
                bulletinTemplate += mdTemplateTitleBottom3;
                break;
        }
    }

    public string Build()
    {
        string errataBulletin = string.Empty;
        AddTitle(ref errataBulletin);
        return string.Empty;
        if (string.IsNullOrEmpty(SnippetTitle)) throw new Exception("No title specified to build errata bulletin");
    }

    public string SnippetTitle;

    public TimeSpan ContentDuration;
}
