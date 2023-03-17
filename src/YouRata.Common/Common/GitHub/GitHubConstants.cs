using System;
using Octokit;

namespace YouRata.Common.GitHub;

public static class GitHubConstants
{
    public static readonly string ProductHeaderName = "YouRata";

    public static readonly TimeSpan RequestTimeout = TimeSpan.FromMilliseconds(1000 * 30);

    public static ProductHeaderValue ProductHeader => new (ProductHeaderName);

    public static string ActionTokenVariable = "ACTION_TOKEN";

    public static string GitHubWorkspaceVariable = "GITHUB_WORKSPACE";

    public static readonly string ApiTokenVariable = "API_TOKEN";

    public static readonly string ErrataBranch = "errata";

    public static readonly string ErrataCheckoutPath = "errata-checkout";
}
