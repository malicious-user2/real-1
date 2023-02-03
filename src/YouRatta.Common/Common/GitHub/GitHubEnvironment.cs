using System;

namespace YouRatta.Common.GitHub;

public class GitHubEnvironment
{
    public GitHubEnvironment()
    {
        this.GITHUB_REPOSITORY = string.Empty;
    }

    public string GITHUB_REPOSITORY { get; set; }
}
