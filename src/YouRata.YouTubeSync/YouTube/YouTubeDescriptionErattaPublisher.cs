using System;
using System.Globalization;
using YouRata.Common.Configurations;

namespace YouRata.YouTubeSync.YouTube;

internal static class YouTubeDescriptionErattaPublisher
{
    public static string GetAmendedDescription(string description, string erattaLink, YouTubeConfiguration config)
    {
        switch (config.ErattaLinkLocation)
        {
            case "top":
                return string.Format(CultureInfo.InvariantCulture, config.ErrataLinkTemplate, erattaLink) +
                    Environment.NewLine +
                    description;
            case "bottom":
                return description +
                    Environment.NewLine +
                    string.Format(CultureInfo.InvariantCulture, config.ErrataLinkTemplate, erattaLink);
        }
        return string.Empty;
    }
}
