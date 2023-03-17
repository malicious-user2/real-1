using System;
using YouRata.Common.GitHub;

namespace YouRata.YouTubeSync.Workflow;

internal class YouTubeSyncWorkflow
{
    private readonly string _workspace;

#pragma warning disable CS8601
#pragma warning disable CS8618
    public YouTubeSyncWorkflow()
    {
        _workspace = Environment
            .GetEnvironmentVariable(GitHubConstants.GitHubWorkspaceVariable);
    }
#pragma warning restore CS8601
#pragma warning restore CS8618

    public string Workspace => _workspace;
}
