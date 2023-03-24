using System;
using System.Globalization;
using YouRata.Common.Configurations;
using YouRata.Common.Proto;
using YouRata.Common.YouTube;

namespace YouRata.YouTubeSync.YouTube;

internal static class YouTubeDescriptionErattaPublisher
{

    public static string GetErrataLink(GitHubActionEnvironment actionEnvironment, string errataBulletinPath)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/blob/{2}/{3}",
            actionEnvironment.EnvGitHubServerUrl,
            actionEnvironment.EnvGitHubRepository,
            actionEnvironment.EnvGitHubRefName,
            errataBulletinPath);
    }

    public static string GetAmendedDescription(string description, string erattaLink, YouTubeConfiguration config)
    {
        string errataNotice = string.Format(CultureInfo.InvariantCulture, config.ErrataLinkTemplate, erattaLink);
        if ((errataNotice.Length + description.Length) > YouTubeConstants.MaxDescriptionLength && config.TruncateDescriptionOverflow)
        {
            description = description.Substring(0, description.Length - errataNotice.Length);
        }
        switch (config.ErattaLinkLocation)
        {
            case "top":
                return errataNotice + Environment.NewLine + description;
            case "bottom":
                return description + Environment.NewLine + errataNotice;
        }
        return string.Empty;
    }
}
