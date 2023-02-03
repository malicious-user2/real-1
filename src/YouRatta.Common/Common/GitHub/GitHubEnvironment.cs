using System;

namespace YouRatta.Common.GitHub;

public class GitHubEnvironment
{
    public GitHubEnvironment()
    {
        this.REPOSITORY = string.Empty;
    }

    public string REPOSITORY { get; set; }
}
