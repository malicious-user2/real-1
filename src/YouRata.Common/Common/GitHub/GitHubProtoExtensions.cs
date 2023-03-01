using System;
using YouRata.Common.Proto;

namespace YouRata.Common.GitHub;

public static class GitHubProtoExtensions
{
    public static GitHubActionEnvironment GetBlank(this GitHubActionEnvironment actionEnvironment)
    {
        return new GitHubActionEnvironment { RateLimitCoreRemaining = 1000 , RateLimitCoreLimit = 1000, RateLimitCoreReset = 0};
    }
}
