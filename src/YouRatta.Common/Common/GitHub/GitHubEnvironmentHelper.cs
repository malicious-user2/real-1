using System;
using Microsoft.Extensions.Configuration;

namespace YouRatta.Common.GitHub;

public sealed class GitHubEnvironmentHelper
{

    public IConfiguration Build()
    {
        return new ConfigurationBuilder()
            .AddEnvironmentVariables("GITHUB_")
            .Build();
    }
}
