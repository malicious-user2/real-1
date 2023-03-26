// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

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
