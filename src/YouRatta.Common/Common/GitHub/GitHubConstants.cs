using System;
using Octokit;

namespace YouRatta.Common.GitHub;

public static class GitHubConstants
{
    public static readonly string ProductHeaderName = "YouRatta";

    public static ProductHeaderValue ProductHeader => new ProductHeaderValue(ProductHeaderName);

    public static string ClientSecretsVariable = "CLIENT_SECRETS";

    public static string ActionTokenVariable = "ACTION_TOKEN";
}
