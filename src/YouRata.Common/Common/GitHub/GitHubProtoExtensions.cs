using System;
using YouRata.Common.Proto;

namespace YouRata.Common.GitHub;

public static class GitHubProtoExtensions
{
    public static GitHubActionEnvironment OverrideRateLimit(this GitHubActionEnvironment actionEnvironment)
    {
        GitHubActionEnvironment newActionEnvironment =
            new GitHubActionEnvironment(actionEnvironment) { RateLimitCoreRemaining = 1000 };
        return newActionEnvironment;
    }
}
