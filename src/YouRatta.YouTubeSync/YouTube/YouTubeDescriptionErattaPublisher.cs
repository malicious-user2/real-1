using System;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using YouRatta.Common.Configurations;

namespace YouRatta.YouTubeSync.YouTube;

internal static class YouTubeDescriptionErattaPublisher
{
    private const string ytTemplateErattaLink1 = "To see eratta for this video: {0}";
    public static string GetAmendedDescription(string description, string erattaLink, YouTubeConfiguration config)
    {
        switch (config.ErattaLinkLocation)
        {
            case "top":
                return string.Format(CultureInfo.InvariantCulture, ytTemplateErattaLink1, erattaLink) +
                    Environment.NewLine +
                    description;
            case "bottom":
                return description +
                    Environment.NewLine +
                    string.Format(CultureInfo.InvariantCulture, ytTemplateErattaLink1, erattaLink);
        }
        return string.Empty;
    }
}
