using System;

namespace YouRatta.Common.GitHub;

public class GitHubEnvironment
{
    public GitHubEnvironment()
    {
        this.Repository = string.Empty;
    }

    public string Repository { get; set; }

    public string Repository_Owner { get; set; }

    public int Run_Attempt { get; set; }

    public int Server_URL { get; set; }

    public string Triggering_Actor { get; set; }
}
