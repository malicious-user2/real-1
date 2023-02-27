using System;
using Octokit;

namespace YouRata.Common.GitHub;

public static class GitHubConstants
{
    public static readonly string ProductHeaderName = "YouRata";

    public static TimeSpan RequestTimeout = TimeSpan.FromMilliseconds(1000 * 30);

    public static ProductHeaderValue ProductHeader => new ProductHeaderValue(ProductHeaderName);

    public static string ActionTokenVariable = "ACTION_TOKEN";

    public static string ApiTokenVariable = "API_TOKEN";

    public static string ErrataBranch = "errata";

    public static string ErrataCheckoutPath = "errata-checkout";
}
